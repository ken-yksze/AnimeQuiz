using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AnimeQuiz.Models
{
    [Index(nameof(StaffName), IsUnique = true)]
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        public required string StaffName { get; set; }

        public ICollection<CharacterVersion>? VoiceActedCharacterVersions { get; set; }

        public ICollection<Music>? SungMusics { get; set; }
    }

    public class StaffDto
    {
        public int StaffId { get; set; }

        [DisplayName("Staff Name")]
        public required string StaffName { get; set; }

        public List<CharacterVersionDto>? VoiceActedCharacterVersionDtos { get; set; }

        public List<MusicDto>? SungMusicDtos { get; set; }
    }

    public class UpdateStaffRequest
    {
        public required string StaffName { get; set; }
    }

    public class AddStaffRequest
    {
        public required string StaffName { get; set; }
    }
}
