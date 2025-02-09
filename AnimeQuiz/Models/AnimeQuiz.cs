namespace AnimeQuiz.Models
{
    public class Question
    {
        public required string QuestionTitle { get; set; }

        public string? ImagePath { get; set; }

        public string? MusicPath { get; set; }

        public required string Answer { get; set; }

        public required List<string> Choices { get; set; } = [];
    }

    public class AnimeQuizDto
    {
        public List<Question> Questions { get; set; } = [];
    }
}
