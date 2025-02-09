using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface ICharacterService
    {
        // base CRUD
        Task<IEnumerable<CharacterDto>> ListCharacters();

        Task<CharacterDto?> FindCharacter(int id);

        Task<ServiceResponse> UpdateCharacter(int id, UpdateCharacterRequest request);

        Task<(ServiceResponse, CharacterDto?)> AddCharacter(AddCharacterRequest request);

        Task<ServiceResponse> DeleteCharacter(int id);

        // related method
        Task<(ServiceResponse, CharacterVersionDto?)> AddVersionToCharacter(int id, AddVersionToCharacterRequest request);

        Task<ServiceResponse> RemoveVersionsFromCharacter(int id, RemoveVersionsFromCharacterRequest request);
    }
}
