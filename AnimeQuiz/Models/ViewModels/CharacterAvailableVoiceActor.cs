namespace AnimeQuiz.Models.ViewModels
{
    public class CharacterAvailableVoiceActor
    {
        public required CharacterVersionDto CharacterVersionDto { get; set; }

        public required IEnumerable<StaffDto> StaffDtos { get; set; }
    }
}
