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
    public class CharacterVersionService : ICharacterVersionService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public CharacterVersionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static CharacterVersionDto ToCharacterVersionDto(CharacterVersion characterVersion)
        {
            if (characterVersion.Character != null)
            {
                characterVersion.Character.CharacterVersions = null;
            }

            return new CharacterVersionDto
            {
                CharacterVersionId = characterVersion.CharacterVersionId,
                CharacterDto = characterVersion.Character == null ? null : CharacterService.ToCharacterDto(characterVersion.Character),
                VersionName = characterVersion.VersionName,
                AnimeDtos = characterVersion.Animes?.Select(a =>
                {
                    a.CharacterVersions = null;
                    return AnimeService.ToAnimeDto(a);
                }).ToList(),
                ImageDtos = characterVersion.Images?.Select(i =>
                {
                    i.CharacterVersion = null;
                    return ImageService.ToImageDto(i);
                }).ToList(),
                VoiceActorDtos = characterVersion.VoiceActors?.Select(va =>
                {
                    va.VoiceActedCharacterVersions = null;
                    return StaffService.ToStaffDto(va);
                }).ToList()
            };
        }

        public async Task<CharacterVersionDto?> FindCharacterVersion(int id)
        {
            // Get a CharacterVersions
            CharacterVersion? characterVersion = await _context.CharacterVersions
                .Include(cv => cv.Character)
                .Include(cv => cv.Images)
                .Include(cv => cv.Animes)
                .Include(cv => cv.VoiceActors)
                .SingleOrDefaultAsync(c => c.CharacterVersionId == id);

            // Convert to Dto
            CharacterVersionDto? characterVersionDto = characterVersion == null ? null : ToCharacterVersionDto(characterVersion);
            return characterVersionDto;
        }

        public async Task<ServiceResponse> UpdateCharacterVersion(int id, UpdateCharacterVersionRequest request)
        {
            ServiceResponse response = new();

            // Validate requested VersionName
            if (string.IsNullOrWhiteSpace(request.VersionName))
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("VersionName cannot be empty.");
                return response;
            }

            // Get a CharacterVersion by id
            CharacterVersion? characterVersion = await _context.CharacterVersions.FindAsync(id);

            // If no such CharacterVersion
            if (characterVersion == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"CharacterVersion with id {id} not found.");
                return response;
            }

            if (characterVersion.VersionName == null)
            {
                response.Status = ServiceStatus.Conflict;
                response.Messages.Add("Cannot rename default CharacterVersion.");
                return response;
            }

            // Modify the existing CharacterVersion
            characterVersion.VersionName = request.VersionName;

            // Flags that the object has changed
            _context.Entry(characterVersion).State = EntityState.Modified;

            try
            {
                // Update the CharacterVersion
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"CharacterVersion with id {id} not found.");
                return response;
            }
            catch (Exception ex)
            {
                // If Exception is caused by SqlException 2601, i.e. Violation of VersionName uniqueness
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    response.Status = ServiceStatus.Conflict;
                    response.Messages.Add($"There exists VersionName {request.VersionName} for Character {characterVersion.CharacterId}.");
                    return response;
                }

                response.Status = ServiceStatus.Error;
                response.Messages.Add("There was an error updating the CharacterVersion.");
                response.Messages.Add(ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Updated;
            return response;
        }

        public async Task<ServiceResponse> AddImagesToCharacterVersion(int id, AddImagesToCharacterVersionRequest request)
        {
            ServiceResponse response = new();
            Boolean invalidData = false;

            // Validate requested ImageFiles length
            if (request.ImageFiles.Count == 0)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one ImageFile when adding to CharacterVersion.");
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

            // CharacterVersion must exist in the first place
            CharacterVersion? characterVersion = await _context.CharacterVersions
                .Include(cv => cv.Images)
                .SingleOrDefaultAsync(cv => cv.CharacterVersionId == id);

            if (characterVersion == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add Images to CharacterVersion because it does not exist.");
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
                            ImageFilename = $"Character_{characterVersion.CharacterVersionId}_{uid}.{imageFileExtension}",
                            CharacterVersionId = characterVersion.CharacterVersionId
                        };

                        characterVersion.Images?.Add(image);

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
                    response.Messages.Add("There was an error adding Images to CharacterVersion.");
                    response.Messages.Add(ex.Message);
                    return response;
                }
            }
        }

        public async Task<ServiceResponse> RemoveImagesFromCharacterVersion(int id, RemoveImagesFromCharacterVersionRequest request)
        {
            ServiceResponse response = new();

            // Validate requested ImageIds length
            if (request.ImageIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one ImageId when deleting from CharacterVersion.");
                return response;
            }

            // CharacterVersion must exist in the first place
            CharacterVersion? characterVersion = await _context.CharacterVersions
                .Include(cv => cv.Images!.Where(i => request.ImageIds.Contains(i.ImageId)))
                .SingleOrDefaultAsync(cv => cv.CharacterVersionId == id);

            if (characterVersion == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete Images from CharacterVersion because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = characterVersion.Images?.Count ?? 0;

            foreach (Image image in characterVersion.Images ?? [])
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
                response.Messages.Add("Error encountered while deleting Images from CharacterVersion.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> AddVoiceActorsToCharacterVersion(int id, AddVoiceActorsToCharacterVersionRequest request)
        {
            ServiceResponse response = new();

            // Validate requested VoiceActorIds length
            if (request.VoiceActorIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one VoiceActorId when adding to Music.");
                return response;
            }

            // CharacterVersion must exist in the first place
            CharacterVersion? characterVersion = await _context.CharacterVersions
                .Include(cv => cv.VoiceActors)
                .SingleOrDefaultAsync(cv => cv.CharacterVersionId == id);

            if (characterVersion == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add VoiceActors to CharacterVersion because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = 0;

            foreach (int voiceActorId in request.VoiceActorIds)
            {
                Staff? staff = await _context.Staffs.FindAsync(voiceActorId);

                if (staff != null)
                {
                    characterVersion.VoiceActors!.Add(staff);
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
                response.Messages.Add("Error encountered while adding VoiceActors to CharacterVersion.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Created;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> RemoveVoiceActorsFromCharacterVersion(int id, RemoveVoiceActorsFromCharacterVersionRequest request)
        {
            ServiceResponse response = new();

            // Validate requested VoiceActorIds length
            if (request.VoiceActorIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one VoiceActorId when deleting from Music.");
                return response;
            }

            // CharacterVersion must exist in the first place
            CharacterVersion? characterVersion = await _context.CharacterVersions
                .Include(cv => cv.VoiceActors!.Where(va => request.VoiceActorIds.Contains(va.StaffId)))
                .SingleOrDefaultAsync(cv => cv.CharacterVersionId == id);

            if (characterVersion == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete VoiceActors from CharacterVersion because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = characterVersion.VoiceActors?.Count ?? 0;

            foreach (Staff voiceActor in characterVersion.VoiceActors ?? [])
            {
                characterVersion.VoiceActors!.Remove(voiceActor);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting VoiceActors from CharacterVersion.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }
    }
}
