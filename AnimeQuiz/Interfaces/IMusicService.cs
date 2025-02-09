using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface IMusicService
    {
        // base CRUD

        // related methods
        Task<ServiceResponse> AddSingersToMusic(int id, AddSingersToMusicRequest request);

        Task<ServiceResponse> RemoveSingersFromMusic(int id, RemoveSingersFromMusicRequest request);
    }
}
