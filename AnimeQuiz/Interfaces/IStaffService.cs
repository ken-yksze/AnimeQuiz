using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface IStaffService
    {
        // base CRUD
        Task<IEnumerable<StaffDto>> ListStaffs();

        Task<StaffDto?> FindStaff(int id);

        Task<ServiceResponse> UpdateStaff(int id, UpdateStaffRequest request);

        Task<(ServiceResponse, StaffDto?)> AddStaff(AddStaffRequest request);

        Task<ServiceResponse> DeleteStaff(int id);

        // related method
    }
}
