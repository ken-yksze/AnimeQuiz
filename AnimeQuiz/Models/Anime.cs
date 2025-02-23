using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AnimeQuiz.Models
{
    [Index(nameof(AnimeName), IsUnique = true)]
    public class Anime
    {
        [Key]
        public int AnimeId { get; set; }

        public required string AnimeName { get; set; }

        // An Anime can have many CharacterVersions
        public ICollection<CharacterVersion>? CharacterVersions { get; set; }

        // An Anime can have many Images
        public ICollection<Image>? Images { get; set; }

        // An Anime can have many Musics
        public ICollection<Music>? Musics { get; set; }
    }

    public class AnimeDto
    {
        public int AnimeId { get; set; }

        [DisplayName("Anime Name")]
        public required string AnimeName { get; set; }

        public List<CharacterVersionDto>? CharacterVersionDtos { get; set; }

        public List<ImageDto>? ImageDtos { get; set; }

        public List<MusicDto>? MusicDtos { get; set; }
    }

    public class UpdateAnimeRequest
    {
        public required string AnimeName { get; set; }
    }

    public class AddAnimeRequest
    {
        public required string AnimeName { get; set; }

        public required List<IFormFile> ImageFiles { get; set; } = [];
    }

    public class AddCharacterVersionsToAnimeRequest
    {
        public required List<int> CharacterVersionIds { get; set; } = [];
    }

    public class RemoveCharacterVersionsFromAnimeRequest
    {
        public required List<int> CharacterVersionIds { get; set; } = [];
    }

    public class AddImagesToAnimeRequest
    {
        public required List<IFormFile> ImageFiles { get; set; } = [];
    }

    public class RemoveImagesFromAnimeRequest
    {
        public required List<int> ImageIds { get; set; } = [];
    }

    public class AddMusicsToAnimeRequest
    {
        public required List<string> MusicNames { get; set; } = [];

        public required List<IFormFile> MusicFiles { get; set; } = [];
    }

    public class RemoveMusicsFromAnimeRequest
    {
        public required List<int> MusicIds { get; set; } = [];
    }
}
