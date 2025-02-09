using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeQuiz.Models
{
    // Unique ImageFilename for accessing specific Image 
    [Index(nameof(ImageFilename), IsUnique = true)]
    public class Image
    {
        [Key]
        public int ImageId { get; set; }

        public required string ImageFilename { get; set; }

        // An Image belongs to one Anime xor one CharacterVersion
        [ForeignKey("Animes")]
        public int? AnimeId { get; set; }
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual Anime? Anime { get; set; }
        [ForeignKey("CharacterVersions")]
        public int? CharacterVersionId { get; set; }
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual CharacterVersion? CharacterVersion { get; set; }
    }

    public class ImageDto
    {
        public int ImageId { get; set; }

        public required string ImagePath { get; set; }

        public AnimeDto? AnimeDto { get; set; }

        public CharacterVersionDto? CharacterVersionDto { get; set; }
    }

    public class UploadImageFile
    {
        public required IFormFile ImageFile { get; set; }

        public required string ImagePath { get; set; }
    }
}
