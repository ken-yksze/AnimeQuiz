using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeQuiz.Models
{
    // Unique Version per Character
    [Index(nameof(CharacterId), nameof(VersionName), IsUnique = true)]
    public class CharacterVersion
    {
        [Key]
        public int CharacterVersionId { get; set; }

        // A CharacterVersion belongs to one Character
        [ForeignKey("Characters")]
        public int CharacterId { get; set; }
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual Character? Character { get; set; }

        public string? VersionName { get; set; }

        // A CharacterVersion can appear in many Animes
        public ICollection<Anime>? Animes { get; set; }

        // A CharacterVersion can have many Images
        public ICollection<Image>? Images { get; set; }

        // A CharacterVersion can have many VoiceActors
        public ICollection<Staff>? VoiceActors { get; set; }
    }

    public class CharacterVersionDto
    {
        public int CharacterVersionId { get; set; }

        public CharacterDto? CharacterDto { get; set; }

        public string? VersionName { get; set; }

        public List<AnimeDto>? AnimeDtos { get; set; }

        public List<ImageDto>? ImageDtos { get; set; }

        public List<StaffDto>? VoiceActorDtos { get; set; }
    }

    public class AddImagesToCharacterVersionRequest
    {
        public required List<IFormFile> ImageFiles { get; set; }
    }

    public class RemoveImagesFromCharacterVersionRequest
    {
        public required List<int> ImageIds { get; set; }
    }

    public class AddVoiceActorsToCharacterVersionRequest
    {
        public required List<int> VoiceActorIds { get; set; }
    }

    public class RemoveVoiceActorsFromCharacterVersionRequest
    {
        public required List<int> VoiceActorIds { get; set; }
    }

    public class UpdateCharacterVersionRequest
    {
        public required string VersionName { get; set; }
    }
}
