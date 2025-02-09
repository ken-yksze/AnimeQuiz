using AnimeQuiz.Data;
using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using HeyRed.Mime;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.RegularExpressions;

namespace AnimeQuiz.Services
{
    public class AnimeService : IAnimeService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public AnimeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static AnimeDto ToAnimeDto(Anime anime)
        {
            return new AnimeDto
            {
                AnimeId = anime.AnimeId,
                AnimeName = anime.AnimeName,
                CharacterVersionDtos = anime.CharacterVersions?.Select(cv =>
                {
                    cv.Animes = null;
                    return CharacterVersionService.ToCharacterVersionDto(cv);
                }).ToList(),
                ImageDtos = anime.Images?.Select(i =>
                {
                    i.Anime = null;
                    return ImageService.ToImageDto(i);
                }).ToList(),
                MusicDtos = anime.Musics?.Select(m =>
                {
                    m.Anime = null;
                    return MusicService.ToMusicDto(m);
                }).ToList()
            };
        }

        public async Task<IEnumerable<AnimeDto>> ListAnimes()
        {
            // All Animes with the first Image as default Image if any
            List<Anime> animes = await _context.Animes.Select(a => new Anime
            {
                AnimeId = a.AnimeId,
                AnimeName = a.AnimeName,
                Images = a.Images!.OrderBy(i => i.ImageId).Take(1).ToList()
            }).ToListAsync();

            // Convert to Dtos
            IEnumerable<AnimeDto> animeDtos = animes.Select(ToAnimeDto);
            return animeDtos;
        }

        public async Task<AnimeDto?> FindAnime(int id)
        {
            // Get an Anime with CharacterVersions, Images, and Musics by id
            Anime? anime = await _context.Animes
                .Include(a => a.CharacterVersions)
                    !.ThenInclude(cv => cv.Character)
                .Include(a => a.Images)
                .Include(a => a.Musics)
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            // Convert to Dto
            AnimeDto? animeDto = anime == null ? null : ToAnimeDto(anime);
            return animeDto;
        }

        public async Task<ServiceResponse> UpdateAnime(int id, UpdateAnimeRequest request)
        {
            ServiceResponse response = new();

            // Validate requested AnimeName
            if (string.IsNullOrWhiteSpace(request.AnimeName))
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("AnimeName cannot be empty.");
                return response;
            }

            // Get an Anime by id
            Anime? anime = await _context.Animes.FindAsync(id);

            // If no such Anime
            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Anime with id {id} not found.");
                return response;
            }

            // Modify the existing Anime
            anime.AnimeName = request.AnimeName;

            // Flags that the object has changed
            _context.Entry(anime).State = EntityState.Modified;

            try
            {
                // Update the Anime
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Anime with id {id} not found.");
                return response;
            }
            catch (DbUpdateException ex)
            {
                // If DbUpdateException is caused by SqlException 2601, i.e. Violation of AnimeName uniqueness
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    response.Status = ServiceStatus.Conflict;
                    response.Messages.Add($"There exists Anime with name {request.AnimeName}.");
                    return response;
                }

                throw;
            }
            catch (Exception ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("There was an error updating the Anime.");
                response.Messages.Add(ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Updated;
            return response;
        }

        public async Task<(ServiceResponse, AnimeDto?)> AddAnime(AddAnimeRequest request)
        {
            ServiceResponse response = new ServiceResponse();
            Boolean invalidData = false;

            // Validate requested AnimeName
            if (string.IsNullOrWhiteSpace(request.AnimeName))
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("AnimeName cannot be empty.");
            }

            // Validate requested ImageFiles length
            if (request.ImageFiles.Count == 0)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one Image when creating Anime.");
            }

            // Validate reqeusted ImageFiles' format
            foreach (IFormFile ImageFile in request.ImageFiles)
            {
                if (!ImageService.ValidContentTypes.Contains(ImageFile.ContentType))
                {
                    invalidData = true;
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add($"Invalid format for file {ImageFile.FileName}, please use jpeg / png / gif / webp.");
                }
            }

            if (invalidData)
            {
                return (response, null);
            }

            // Create instance of Anime
            Anime anime = new()
            {
                AnimeName = request.AnimeName,
                Images = []
            };

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Animes.Add(anime);
                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    List<UploadImageFile> uploadImageFiles = [];

                    foreach (IFormFile ImageFile in request.ImageFiles)
                    {
                        string uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                        string imageFileExtension = MimeTypesMap.GetExtension(ImageFile.ContentType);

                        Image image = new()
                        {
                            ImageFilename = $"Anime_{anime.AnimeId}_{uid}.{imageFileExtension}",
                            AnimeId = anime.AnimeId
                        };

                        anime.Images.Add(image);

                        uploadImageFiles.Add(new()
                        {
                            ImageFile = ImageFile,
                            ImagePath = Path.Combine("wwwroot/assets/images/", image.ImageFilename)
                        });
                    }

                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    foreach (UploadImageFile uploadImageFile in uploadImageFiles)
                    {
                        using (FileStream imageStream = File.Create(uploadImageFile.ImagePath))
                        {
                            await uploadImageFile.ImageFile.CopyToAsync(imageStream);
                        }

                        if (!File.Exists(uploadImageFile.ImagePath))
                        {
                            throw new UploadException("There was an error uploading the Image.");
                        }
                    }

                    // Commit changes
                    await transaction.CommitAsync();

                    response.Status = ServiceStatus.Created;
                    return (response, ToAnimeDto(anime));
                }
                catch (Exception ex)
                {
                    // Rollback all changes
                    await transaction.RollbackAsync();

                    // If Exception is caused by SqlException 2601, i.e. Violation of AnimeName uniqueness
                    if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                    {
                        response.Status = ServiceStatus.Conflict;
                        response.Messages.Add($"There exists Anime with name {request.AnimeName}.");
                        return (response, null);
                    }

                    // Delete all uploaded images
                    DirectoryInfo dir = new("wwwroot/assets/images/");

                    foreach (FileInfo file in dir.EnumerateFiles($"Anime_{anime.AnimeId}_*"))
                    {
                        file.Delete();
                    }

                    response.Status = ServiceStatus.Error;
                    response.Messages.Add("There was an error adding the Anime.");
                    response.Messages.Add(ex.Message);
                    return (response, null);
                }
            }
        }

        public async Task<ServiceResponse> DeleteAnime(int id)
        {
            ServiceResponse response = new();

            // Anime must exist in the first place
            Anime? anime = await _context.Animes.FindAsync(id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Anime cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Animes.Remove(anime);
                await _context.SaveChangesAsync();

                // Delete all uploaded images
                DirectoryInfo imageDir = new("wwwroot/assets/images/");

                foreach (FileInfo file in imageDir.EnumerateFiles($"Anime_{anime.AnimeId}_*"))
                {
                    file.Delete();
                }

                // Delete all uploaded musics
                DirectoryInfo musicDir = new("wwwroot/assets/musics/");

                foreach (FileInfo file in musicDir.EnumerateFiles($"Anime_{anime.AnimeId}_*"))
                {
                    file.Delete();
                }
            }
            catch (Exception)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the Anime.");
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            return response;
        }

        public async Task<ServiceResponse> AddCharacterVersionsToAnime(int id, AddCharacterVersionsToAnimeRequest request)
        {
            ServiceResponse response = new();

            // Validate requested CharacterVersionIds length
            if (request.CharacterVersionIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one CharacterVersionId when adding to Music.");
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.CharacterVersions)
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add CharacterVersions to Anime because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = 0;

            foreach (int characterVersionId in request.CharacterVersionIds)
            {
                CharacterVersion? characterVersion = await _context.CharacterVersions.FindAsync(characterVersionId);

                if (characterVersion != null)
                {
                    anime.CharacterVersions!.Add(characterVersion);
                    affectedRecordsNumber++;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while adding CharacterVersions to Anime.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Created;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> RemoveCharacterVersionsFromAnime(int id, RemoveCharacterVersionsFromAnimeRequest request)
        {
            ServiceResponse response = new();

            // Validate requested CharacterVersionIds length
            if (request.CharacterVersionIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one CharacterVersionId when deleting from Music.");
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.CharacterVersions!.Where(cv => request.CharacterVersionIds.Contains(cv.CharacterVersionId)))
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete CharacterVersions from Anime because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = anime.CharacterVersions?.Count ?? 0;

            foreach (CharacterVersion characterVersion in anime.CharacterVersions ?? [])
            {
                anime.CharacterVersions!.Remove(characterVersion);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting CharacterVersions from Anime.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> AddImagesToAnime(int id, AddImagesToAnimeRequest request)
        {
            ServiceResponse response = new();
            Boolean invalidData = false;

            // Validate requested ImageFiles length
            if (request.ImageFiles.Count == 0)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one ImageFile when adding to Anime.");
            }

            // Validate reqeusted ImageFiles' format
            foreach (IFormFile ImageFile in request.ImageFiles)
            {
                if (!ImageService.ValidContentTypes.Contains(ImageFile.ContentType))
                {
                    invalidData = true;
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add($"Invalid format for file {ImageFile.FileName}, please use jpeg / png / gif / webp.");
                }
            }

            if (invalidData)
            {
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.Images)
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add Images to Anime because it does not exist.");
                return response;
            }

            List<UploadImageFile> uploadImageFiles = [];

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (IFormFile ImageFile in request.ImageFiles)
                    {
                        string uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                        string imageFileExtension = MimeTypesMap.GetExtension(ImageFile.ContentType);

                        Image image = new()
                        {
                            ImageFilename = $"Anime_{anime.AnimeId}_{uid}.{imageFileExtension}",
                            AnimeId = anime.AnimeId
                        };

                        anime.Images?.Add(image);

                        uploadImageFiles.Add(new()
                        {
                            ImageFile = ImageFile,
                            ImagePath = Path.Combine("wwwroot/assets/images/", image.ImageFilename)
                        });
                    }

                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    foreach (UploadImageFile uploadImageFile in uploadImageFiles)
                    {
                        using (FileStream imageStream = File.Create(uploadImageFile.ImagePath))
                        {
                            await uploadImageFile.ImageFile.CopyToAsync(imageStream);
                        }

                        if (!File.Exists(uploadImageFile.ImagePath))
                        {
                            throw new UploadException("There was an error uploading the Image.");
                        }
                    }

                    // Commit changes
                    await transaction.CommitAsync();

                    response.Status = ServiceStatus.Created;
                    response.Messages.Add($"{uploadImageFiles.Count} records are added.");
                    return response;
                }
                catch (Exception ex)
                {
                    // Rollback all changes
                    await transaction.RollbackAsync();

                    // Delete all images uploaded by this request
                    foreach (UploadImageFile uploadImageFile in uploadImageFiles)
                    {
                        File.Delete(uploadImageFile.ImagePath);
                    }

                    response.Status = ServiceStatus.Error;
                    response.Messages.Add("There was an error adding Images to Anime.");
                    response.Messages.Add(ex.Message);
                    return response;
                }
            }
        }

        public async Task<ServiceResponse> RemoveImagesFromAnime(int id, RemoveImagesFromAnimeRequest request)
        {
            ServiceResponse response = new();

            // Validate requested ImageIds length
            if (request.ImageIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one ImageId when deleting from Anime.");
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.Images!.Where(i => request.ImageIds.Contains(i.ImageId)))
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete Images from Anime because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = anime.Images?.Count ?? 0;

            foreach (Image image in anime.Images ?? [])
            {
                File.Delete(Path.Combine("wwwroot/assets/images/", image.ImageFilename));
                _context.Images.Remove(image);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting Images from Anime.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> AddMusicsToAnime(int id, AddMusicsToAnimeRequest request)
        {
            ServiceResponse response = new();
            Boolean invalidData = false;

            // Validate if two list is of the same length
            if (request.MusicNames.Count != request.MusicFiles.Count)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("No. of MusicNames should be the same as no. of MusicFiles.");
            }

            // Validate requested MusicFiles length
            if (request.MusicFiles.Count == 0)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one MusicFile when adding to Anime.");
            }

            // Validate reqeusted MusicNames' format
            for (int i = 0; i < request.MusicNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(request.MusicNames[i]))
                {
                    invalidData = true;
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add($"MusicName at pos {i} cannot be empty.");
                }
            }

            // Validate reqeusted MusicFiles' format
            foreach (IFormFile MusicFile in request.MusicFiles)
            {
                if (!MusicService.ValidContentTypes.Contains(MusicFile.ContentType))
                {
                    invalidData = true;
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add($"Invalid format for file {MusicFile.FileName}, please use mp3 / mp4 / wav / flac.");
                }
            }

            if (invalidData)
            {
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.Musics)
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add Musics to Anime because it does not exist.");
                return response;
            }

            List<UploadMusicFile> uploadMusicFiles = [];

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < request.MusicFiles.Count; i++)
                    {
                        string uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                        string musicFileExtension = MimeTypesMap.GetExtension(request.MusicFiles[i].ContentType);

                        Music music = new()
                        {
                            MusicName = request.MusicNames[i],
                            MusicFilename = $"Anime_{anime.AnimeId}_{uid}.{musicFileExtension}",
                            AnimeId = anime.AnimeId
                        };

                        anime.Musics?.Add(music);

                        uploadMusicFiles.Add(new()
                        {
                            MusicFile = request.MusicFiles[i],
                            MusicPath = Path.Combine("wwwroot/assets/musics/", music.MusicFilename)
                        });
                    }

                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    foreach (UploadMusicFile uploadMusicFile in uploadMusicFiles)
                    {
                        using (FileStream musicStream = File.Create(uploadMusicFile.MusicPath))
                        {
                            await uploadMusicFile.MusicFile.CopyToAsync(musicStream);
                        }

                        if (!File.Exists(uploadMusicFile.MusicPath))
                        {
                            throw new UploadException("There was an error uploading the Music.");
                        }
                    }

                    // Commit changes
                    await transaction.CommitAsync();

                    response.Status = ServiceStatus.Created;
                    response.Messages.Add($"{uploadMusicFiles.Count} records are added.");
                    return response;
                }
                catch (Exception ex)
                {
                    // Rollback all changes
                    await transaction.RollbackAsync();

                    // Delete all images uploaded by this request
                    foreach (UploadMusicFile uploadMusicFile in uploadMusicFiles)
                    {
                        File.Delete(uploadMusicFile.MusicPath);
                    }

                    response.Status = ServiceStatus.Error;
                    response.Messages.Add("There was an error adding Musics to Anime.");
                    response.Messages.Add(ex.Message);
                    return response;
                }
            }
        }

        public async Task<ServiceResponse> RemoveMusicsFromAnime(int id, RemoveMusicsFromAnimeRequest request)
        {
            ServiceResponse response = new();

            // Validate requested MusicIds length
            if (request.MusicIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one MusicId when deleting from Anime.");
                return response;
            }

            // Anime must exist in the first place
            Anime? anime = await _context.Animes
                .Include(a => a.Musics!.Where(i => request.MusicIds.Contains(i.MusicId)))
                .SingleOrDefaultAsync(a => a.AnimeId == id);

            if (anime == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete Musics from Anime because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = anime.Musics?.Count ?? 0;

            foreach (Music music in anime.Musics ?? [])
            {
                File.Delete(Path.Combine("wwwroot/assets/musics/", music.MusicFilename));
                _context.Musics.Remove(music);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting Musics from Anime.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }
    }
}
