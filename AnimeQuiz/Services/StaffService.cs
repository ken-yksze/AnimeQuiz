using AnimeQuiz.Data;
using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AnimeQuiz.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public StaffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static StaffDto ToStaffDto(Staff staff)
        {
            return new StaffDto
            {
                StaffId = staff.StaffId,
                StaffName = staff.StaffName,
                VoiceActedCharacterVersionDtos = staff.VoiceActedCharacterVersions?.Select(vacv =>
                {
                    vacv.VoiceActors = null;
                    return CharacterVersionService.ToCharacterVersionDto(vacv);
                }).ToList(),
                SungMusicDtos = staff.SungMusics?.Select(sm =>
                {
                    sm.Singers = null;
                    return MusicService.ToMusicDto(sm);
                }).ToList()
            };
        }

        public async Task<IEnumerable<StaffDto>> ListStaffs()
        {
            // All Staffs
            List<Staff> staffs = await _context.Staffs.ToListAsync();

            // Convert to Dtos
            IEnumerable<StaffDto> staffDtos = staffs.Select(ToStaffDto);
            return staffDtos;
        }

        public async Task<StaffDto?> FindStaff(int id)
        {
            // Get a Staff with CharacterVersions and Musics by id
            Staff? staff = await _context.Staffs
                .Select(s => new Staff
                {
                    StaffId = s.StaffId,
                    StaffName = s.StaffName,
                    SungMusics = s.SungMusics,
                    VoiceActedCharacterVersions = s.VoiceActedCharacterVersions
                        !.Select(vacv => new CharacterVersion
                        {
                            CharacterVersionId = vacv.CharacterVersionId,
                            CharacterId = vacv.CharacterId,
                            Character = vacv.Character,
                            VersionName = vacv.VersionName,
                            Images = vacv.Images
                                !.OrderBy(i => i.ImageId)
                                .Take(1)
                                .ToList()
                        })
                        .ToList()
                })
                .SingleOrDefaultAsync(a => a.StaffId == id);

            // Convert to Dto
            StaffDto? staffDto = staff == null ? null : ToStaffDto(staff);
            return staffDto;
        }

        public async Task<ServiceResponse> UpdateStaff(int id, UpdateStaffRequest request)
        {
            ServiceResponse response = new();

            // Validate requested StaffName
            if (string.IsNullOrWhiteSpace(request.StaffName))
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("StaffName cannot be empty.");
                return response;
            }

            // Get an Staff by id
            Staff? staff = await _context.Staffs.FindAsync(id);

            // If no such Staff
            if (staff == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Staff with id {id} not found.");
                return response;
            }

            // Modify the existing Staff
            staff.StaffName = request.StaffName;

            // Flags that the object has changed
            _context.Entry(staff).State = EntityState.Modified;

            try
            {
                // Update the Staff
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add($"Staff with id {id} not found.");
                return response;
            }
            catch (DbUpdateException ex)
            {
                // If DbUpdateException is caused by SqlException 2601, i.e. Violation of StaffName uniqueness
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    response.Status = ServiceStatus.Conflict;
                    response.Messages.Add($"There exists Staff with name {request.StaffName}.");
                    return response;
                }

                throw;
            }
            catch (Exception ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("There was an error updating the Staff.");
                response.Messages.Add(ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Updated;
            return response;
        }

        public async Task<(ServiceResponse, StaffDto?)> AddStaff(AddStaffRequest request)
        {
            ServiceResponse response = new ServiceResponse();

            // Validate requested StaffName
            if (string.IsNullOrWhiteSpace(request.StaffName))
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("StaffName cannot be empty.");
                return (response, null);
            }

            // Create instance of Staff
            Staff staff = new()
            {
                StaffName = request.StaffName
            };

            try
            {
                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // If Exception is caused by SqlException 2601, i.e. Violation of StaffName uniqueness
                if (ex.InnerException is SqlException sqlException && sqlException.Number == 2601)
                {
                    response.Status = ServiceStatus.Conflict;
                    response.Messages.Add($"There exists Staff with name {request.StaffName}.");
                    return (response, null);
                }

                response.Status = ServiceStatus.Error;
                response.Messages.Add("There was an error adding the Staff.");
                response.Messages.Add(ex.Message);
                return (response, null);
            }

            response.Status = ServiceStatus.Created;
            return (response, ToStaffDto(staff));
        }

        public async Task<ServiceResponse> DeleteStaff(int id)
        {
            ServiceResponse response = new();

            // Staff must exist in the first place
            Staff? staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Staff cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the Staff.");
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            return response;
        }
    }
}
