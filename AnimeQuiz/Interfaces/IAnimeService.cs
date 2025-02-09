using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface IAnimeService
    {
        // base CRUD
        Task<IEnumerable<AnimeDto>> ListAnimes();

        Task<AnimeDto?> FindAnime(int id);

        Task<ServiceResponse> UpdateAnime(int id, UpdateAnimeRequest request);

        Task<(ServiceResponse, AnimeDto?)> AddAnime(AddAnimeRequest request);

        Task<ServiceResponse> DeleteAnime(int id);

        // related methods
        Task<ServiceResponse> AddCharacterVersionsToAnime(int id, AddCharacterVersionsToAnimeRequest request);

        Task<ServiceResponse> RemoveCharacterVersionsFromAnime(int id, RemoveCharacterVersionsFromAnimeRequest request);

        Task<ServiceResponse> AddImagesToAnime(int id, AddImagesToAnimeRequest request);

        Task<ServiceResponse> RemoveImagesFromAnime(int id, RemoveImagesFromAnimeRequest request);

        Task<ServiceResponse> AddMusicsToAnime(int id, AddMusicsToAnimeRequest request);

        Task<ServiceResponse> RemoveMusicsFromAnime(int id, RemoveMusicsFromAnimeRequest request);
    }
}
