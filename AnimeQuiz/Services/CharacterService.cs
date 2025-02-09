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
    public class CharacterService : ICharacterService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public CharacterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static CharacterDto ToCharacterDto(Character character)
        {
            return new CharacterDto
            {
                CharacterId = character.CharacterId,
                CharacterName = character.CharacterName,
                CharacterVersionDtos = character.CharacterVersions?.Select(cv =>
                {
                    cv.Character = null;
                    return CharacterVersionService.ToCharacterVersionDto(cv);
                }).ToList()
            };
        }

        public async Task<IEnumerable<CharacterDto>> ListCharacters()
        {
            // All Characters with the first CharacterVersion as default CharacterVersion
            // The default CharacterVersion will include the first Image as default Image if any
            List<Character> characters = await _context.Characters
                .Select(c => new Character
                {
                    CharacterId = c.CharacterId,
                    CharacterName = c.CharacterName,
                    CharacterVersions = c.CharacterVersions
                        !.Where(cv => cv.VersionName == null)
                        .Select(cv => new CharacterVersion
                        {
                            CharacterVersionId = cv.CharacterVersionId,
                            CharacterId = cv.CharacterId,
                            VersionName = cv.VersionName,
                            Images = cv.Images
                                !.OrderBy(i => i.ImageId)
                                .Take(1)
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            // Convert to Dtos
            IEnumerable<CharacterDto> characterDtos = characters.Select(ToCharacterDto);
            return characterDtos;
        }

        public async Task<CharacterDto?> FindCharacter(int id)
        {
            // Get a Character with CharacterVersions
            // A CharacterVersion will also include the first Image as default image if any
            Character? character = await _context.Characters
                .Include(c => c.CharacterVersions)
                .Select(c => new Character
                {
                    CharacterId = c.CharacterId,
                    CharacterName = c.CharacterName,
                    CharacterVersions = c.CharacterVersions!.Select(cv => new CharacterVersion
                    {
                        CharacterVersionId = cv.CharacterVersionId,
                        CharacterId = cv.CharacterId,
                        VersionName = cv.VersionName,
                        Images = cv.Images!.OrderBy(i => i.ImageId).Take(1).ToList()
                    }
                    ).ToList()
                })
                .SingleOrDefaultAsync(c => c.CharacterId == id);

            // Convert to Dto
            CharacterDto? characterDto = character == null ? null : ToCharacterDto(character);
            return characterDto;
        }

        public async Task<ServiceResponse> UpdateCharacter(int id, UpdateCharacterRequest request)
        {
            ServiceResponse response = new();

            // Validate requested CharacterName
            if (string.IsNullOrWhiteSpace(request.CharacterName))
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("CharacterName cannot be empty.");
                return response;
            }

            // Get a character by id
            Character? character = await _context.Characters.FindAsync(id);

            // If no such Character
            if (character == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Character with id {id} not found.");
                return response;
            }

            // Modify the existing Character
            character.CharacterName = request.CharacterName;

            // Flags that the object has changed
            _context.Entry(character).State = EntityState.Modified;

            try
            {
                // Update the Character
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Character with id {id} not found.");
                return response;
            }
            catch (Exception ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("There was an error updating the Character.");
                response.Messages.Add(ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Updated;
            return response;
        }

        public async Task<(ServiceResponse, CharacterDto?)> AddCharacter(AddCharacterRequest request)
        {
            ServiceResponse response = new ServiceResponse();
            Boolean invalidData = false;

            // Validate requested CharacterName
            if (string.IsNullOrWhiteSpace(request.CharacterName))
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("CharacterName cannot be empty.");
            }

            // Validate requested ImageFiles length
            if (request.ImageFiles.Count == 0)
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one Image when creating Character.");
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

            // Create instance of Character
            Character character = new()
            {
                CharacterName = request.CharacterName,
                CharacterVersions = []
            };

            CharacterVersion characterVersion = new()
            {
                Images = []
            };

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Characters.Add(character);
                    character.CharacterVersions.Add(characterVersion);
                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    List<UploadImageFile> uploadImageFiles = [];

                    foreach (IFormFile ImageFile in request.ImageFiles)
                    {
                        string uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                        string imageFileExtension = MimeTypesMap.GetExtension(ImageFile.ContentType);

                        Image image = new()
                        {
                            ImageFilename = $"Character_{characterVersion.CharacterVersionId}_{uid}.{imageFileExtension}",
                            CharacterVersionId = characterVersion.CharacterVersionId
                        };

                        characterVersion.Images.Add(image);

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
                    return (response, ToCharacterDto(character));
                }
                catch (Exception ex)
                {
                    // Rollback all changes
                    await transaction.RollbackAsync();

                    // Delete all uploaded images
                    DirectoryInfo dir = new("wwwroot/assets/images/");

                    foreach (FileInfo file in dir.EnumerateFiles($"Character_{characterVersion.CharacterVersionId}_*"))
                    {
                        file.Delete();
                    }

                    response.Status = ServiceStatus.Error;
                    response.Messages.Add("There was an error adding the Character.");
                    response.Messages.Add(ex.Message);
                    return (response, null);
                }
            }
        }

        public async Task<ServiceResponse> DeleteCharacter(int id)
        {
            ServiceResponse response = new();

            // Character must exist in the first place
            Character? character = await _context.Characters
                .Include(c => c.CharacterVersions)
                .SingleOrDefaultAsync(c => c.CharacterId == id);

            if (character == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Character cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();

                // Delete all uploaded images
                DirectoryInfo imageDir = new("wwwroot/assets/images/");

                foreach (CharacterVersion characterVersion in character.CharacterVersions ?? [])
                {
                    string filenamePrefix = $"Character_{characterVersion.CharacterVersionId}_*";

                    foreach (FileInfo file in imageDir.EnumerateFiles(filenamePrefix))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the Character.");
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            return response;
        }

        public async Task<(ServiceResponse, CharacterVersionDto?)> AddVersionToCharacter(int id, AddVersionToCharacterRequest request)
        {
            ServiceResponse response = new ServiceResponse();
            Boolean invalidData = false;

            // Validate requested VersionName
            if (string.IsNullOrWhiteSpace(request.VersionName))
            {
                invalidData = true;
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("VersionName cannot be empty.");
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

            // Character must exist in the first place
            Character? character = await _context.Characters
                .Include(c => c.CharacterVersions)
                .SingleOrDefaultAsync(c => c.CharacterId == id);

            if (character == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add Version to Character because it does not exist.");
                return (response, null);
            }

            CharacterVersion characterVersion = new()
            {
                CharacterId = character.CharacterId,
                VersionName = request.VersionName,
                Images = []
            };

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    character.CharacterVersions!.Add(characterVersion);
                    // Save changes data in context
                    await _context.SaveChangesAsync();

                    List<UploadImageFile> uploadImageFiles = [];

                    foreach (IFormFile ImageFile in request.ImageFiles)
                    {
                        string uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                        string imageFileExtension = MimeTypesMap.GetExtension(ImageFile.ContentType);

                        Image image = new()
                        {
                            ImageFilename = $"Character_{characterVersion.CharacterVersionId}_{uid}.{imageFileExtension}",
                            CharacterVersionId = characterVersion.CharacterVersionId
                        };

                        characterVersion.Images.Add(image);

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
                    return (response, CharacterVersionService.ToCharacterVersionDto(characterVersion));
                }
                catch (Exception ex)
                {
                    // Rollback all changes
                    await transaction.RollbackAsync();

                    // If Exception is caused by SqlException 2601, i.e. Violation of VersionName uniqueness
                    if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                    {
                        response.Status = ServiceStatus.Conflict;
                        response.Messages.Add($"There exists VersionName {request.VersionName} for Character {id}.");
                        return (response, null);
                    }

                    // Delete all uploaded images
                    DirectoryInfo imageDir = new("wwwroot/assets/images/");

                    foreach (FileInfo file in imageDir.EnumerateFiles($"Character_{characterVersion.CharacterVersionId}_*"))
                    {
                        file.Delete();
                    }

                    response.Status = ServiceStatus.Error;
                    response.Messages.Add("There was an error adding Version to Character.");
                    response.Messages.Add(ex.Message);
                    return (response, null);
                }
            }
        }

        public async Task<ServiceResponse> RemoveVersionsFromCharacter(int id, RemoveVersionsFromCharacterRequest request)
        {
            ServiceResponse response = new();

            // Validate requested CharacterVersionIds length
            if (request.CharacterVersionIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one CharacterVersionId when deleting from Anime.");
                return response;
            }

            // Character must exist in the first place
            Character? character = await _context.Characters
                .AsNoTracking()  // Prevents tracking-related issues
                .Include(c => c.CharacterVersions!
                    .Where(cv => request.CharacterVersionIds.Contains(cv.CharacterVersionId)))
                .SingleOrDefaultAsync(c => c.CharacterId == id);

            if (character == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete Versions from Character because it does not exist.");
                return response;
            }

            CharacterVersion? defaultVersion = await _context.CharacterVersions.SingleOrDefaultAsync(cv => cv.CharacterId == character.CharacterId && cv.VersionName == null);

            if (defaultVersion == null)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Unexpected error, DefaultVersion should exist when Character exists.");
                return response;
            }

            // If attempting to delete default version by this api
            if (request.CharacterVersionIds.Contains(defaultVersion.CharacterVersionId))
            {
                response.Status = ServiceStatus.Conflict;
                response.Messages.Add("Default version cannot be deleted separately, it will be deleted when Character is deleted.");
                return response;
            }

            int affectedRecordsNumber = character.CharacterVersions?.Count ?? 0;
            DirectoryInfo imageDir = new("wwwroot/assets/images/");

            foreach (CharacterVersion characterVersion in character.CharacterVersions ?? [])
            {
                string filenamePrefix = $"Character_{characterVersion.CharacterVersionId}_*";

                foreach (FileInfo file in imageDir.EnumerateFiles(filenamePrefix))
                {
                    file.Delete();
                }

                _context.Entry(characterVersion).State = EntityState.Deleted;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting Versions from Character.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }
    }
}
