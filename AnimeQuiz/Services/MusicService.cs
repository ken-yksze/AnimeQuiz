using AnimeQuiz.Data;
using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeQuiz.Services
{
    public class MusicService : IMusicService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public MusicService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static MusicDto ToMusicDto(Music music)
        {
            if (music.Anime != null)
            {
                music.Anime.Musics = null;
            }

            return new MusicDto
            {
                MusicId = music.MusicId,
                MusicName = music.MusicName,
                MusicPath = $"/assets/musics/{music.MusicFilename}",
                AnimeDto = music.Anime == null ? null : AnimeService.ToAnimeDto(music.Anime),
                SingerDtos = music.Singers?.Select(s =>
                {
                    s.SungMusics = null;
                    return StaffService.ToStaffDto(s);
                }).ToList()
            };
        }

        public static readonly List<string> ValidContentTypes = ["audio/mpeg", "audio/mp4", "audio/wav", "audio/flac"];

        public async Task<ServiceResponse> AddSingersToMusic(int id, AddSingersToMusicRequest request)
        {
            ServiceResponse response = new();

            // Validate requested SingerIds length
            if (request.SingerIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one SingerId when adding to Music.");
                return response;
            }

            // Music must exist in the first place
            Music? music = await _context.Musics
                .Include(m => m.Singers)
                .SingleOrDefaultAsync(m => m.MusicId == id);

            if (music == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot add Singers to Music because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = 0;

            foreach (int singerId in request.SingerIds)
            {
                Staff? staff = await _context.Staffs.FindAsync(singerId);

                if (staff != null)
                {
                    music.Singers!.Add(staff);
                    affectedRecordsNumber++;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while adding Singers to Music.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Created;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }

        public async Task<ServiceResponse> RemoveSingersFromMusic(int id, RemoveSingersFromMusicRequest request)
        {
            ServiceResponse response = new();

            // Validate requested SingerIds length
            if (request.SingerIds.Count == 0)
            {
                response.Status = ServiceStatus.BadRequest;
                response.Messages.Add("Must include at least one SingerId when deleting from Music.");
                return response;
            }

            // Music must exist in the first place
            Music? music = await _context.Musics
                .Include(m => m.Singers!.Where(s => request.SingerIds.Contains(s.StaffId)))
                .SingleOrDefaultAsync(m => m.MusicId == id);

            if (music == null)
            {
                response.Status = ServiceStatus.NotFound;
                response.Messages.Add("Cannot delete Singers from Music because it does not exist.");
                return response;
            }

            int affectedRecordsNumber = music.Singers?.Count ?? 0;

            foreach (Staff singer in music.Singers ?? [])
            {
                music.Singers!.Remove(singer);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception Ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting Singers from Music.");
                response.Messages.Add(Ex.Message);
                return response;
            }

            response.Status = ServiceStatus.Deleted;
            response.Messages.Add($"{affectedRecordsNumber} records are affected.");
            return response;
        }
    }
}
