using AnimeQuiz.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnimeQuiz.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Anime> Animes { get; set; }

        public DbSet<Character> Characters { get; set; }

        public DbSet<CharacterVersion> CharacterVersions { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<Music> Musics { get; set; }

        public DbSet<Staff> Staffs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // In Images table, enforce ((AnimeId IS NULL) XOR (CharacterVersionId IS NULL)) ≡ True
            modelBuilder.Entity<Image>().ToTable(i => i.HasCheckConstraint("CK_Images_AnimeId_CharacterVersionId", "(AnimeId IS NULL OR CharacterVersionId IS NULL) AND (AnimeId IS NOT NULL OR CharacterVersionId IS NOT NULL)"));
        }
    }
}
