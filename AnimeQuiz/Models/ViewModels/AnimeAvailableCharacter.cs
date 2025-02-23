namespace AnimeQuiz.Models.ViewModels
{
    public class AnimeAvailableCharacter
    {
        public required AnimeDto AnimeDto { get; set; }

        public required IEnumerable<CharacterVersionDto> CharacterVersionDtos { get; set; }
    }
}
