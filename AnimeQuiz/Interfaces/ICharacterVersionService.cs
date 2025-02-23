using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface ICharacterVersionService
    {
        // base CRUD
        Task<CharacterVersionDto?> FindCharacterVersion(int id);

        Task<ServiceResponse> UpdateCharacterVersion(int id, UpdateCharacterVersionRequest request);

        // related method
        Task<ServiceResponse> AddImagesToCharacterVersion(int id, AddImagesToCharacterVersionRequest request);

        Task<ServiceResponse> RemoveImagesFromCharacterVersion(int id, RemoveImagesFromCharacterVersionRequest request);

        Task<ServiceResponse> AddVoiceActorsToCharacterVersion(int id, AddVoiceActorsToCharacterVersionRequest request);

        Task<ServiceResponse> RemoveVoiceActorsFromCharacterVersion(int id, RemoveVoiceActorsFromCharacterVersionRequest request);

        Task<IEnumerable<StaffDto>> FindAvailableVoiceActorsForCharacter(int id);
    }
}
