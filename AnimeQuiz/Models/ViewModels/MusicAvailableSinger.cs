namespace AnimeQuiz.Models.ViewModels
{
    public class MusicAvailableSinger
    {
        public required MusicDto MusicDto { get; set; }

        public required IEnumerable<StaffDto> StaffDtos { get; set; }
    }
}
