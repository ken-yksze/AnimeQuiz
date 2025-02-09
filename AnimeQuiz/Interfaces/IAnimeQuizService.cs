using AnimeQuiz.Models;

namespace AnimeQuiz.Interfaces
{
    public interface IAnimeQuizService
    {
        Task<(ServiceResponse, AnimeQuizDto?)> GenerateAnimeQuiz(int numOfQuestions);
    }
}
