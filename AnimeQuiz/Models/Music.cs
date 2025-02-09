using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeQuiz.Models
{
    // Unique MusicName per Anime
    [Index(nameof(MusicName), nameof(AnimeId), IsUnique = true)]
    // Unique MusicFilename for accessing specific Music 
    [Index(nameof(MusicFilename), IsUnique = true)]
    public class Music
    {
        [Key]
        public int MusicId { get; set; }

        public required string MusicName { get; set; }

        public required string MusicFilename { get; set; }

        // A Music belongs to one Anime
        [ForeignKey("Animes")]
        public int AnimeId { get; set; }
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual Anime? Anime { get; set; }

        // A Music can have many Singers
        public ICollection<Staff>? Singers { get; set; }
    }

    public class MusicDto
    {
        public int MusicId { get; set; }

        public required string MusicName { get; set; }

        public required string MusicPath { get; set; }

        public AnimeDto? AnimeDto { get; set; }

        public List<StaffDto>? SingerDtos { get; set; }
    }

    public class UploadMusicFile
    {
        public required IFormFile MusicFile { get; set; }

        public required string MusicPath { get; set; }
    }

    public class AddSingersToMusicRequest
    {
        public required List<int> SingerIds { get; set; }
    }

    public class RemoveSingersFromMusicRequest
    {
        public required List<int> SingerIds { get; set; }
    }
}
