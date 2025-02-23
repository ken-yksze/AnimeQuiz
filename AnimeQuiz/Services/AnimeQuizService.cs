using AnimeQuiz.Data;
using AnimeQuiz.Interfaces;
using AnimeQuiz.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace AnimeQuiz.Services
{
    public class AnimeQuizService : IAnimeQuizService
    {
        private readonly ApplicationDbContext _context;
        private static readonly Random _random = new Random();

        // dependency injection of database context
        public AnimeQuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public T? RandomChoice<T>(List<T>? list)
        {
            if (list is null || list.Count == 0)
            {
                return default;
            }

            return list[_random.Next(list.Count)];
        }

        public enum CharacterImageQuestionType { CharacterName, VoiceActorName }
        public enum AnimeMusicQuestionType { MusicName, SingerName }

        public async Task<int> GetTotalAvailable()
        {
            // Get the number of available questions in db per type and total available
            int numOfAnimeImages = await _context.Images.Where(i => i.AnimeId != null).CountAsync();
            int numOfCharacterImages = await _context.Images.Where(i => i.CharacterVersionId != null).CountAsync();
            int numOfAnimeMusics = await _context.Musics.CountAsync();
            int totalAvailable = numOfAnimeImages + numOfCharacterImages + numOfAnimeMusics;

            return totalAvailable;
        }

        public async Task<(ServiceResponse, AnimeQuizDto?)> GenerateAnimeQuiz(int numOfQuestions)
        {
            ServiceResponse response = new();
            AnimeQuizDto animeQuizDto = new();

            try
            {
                if (numOfQuestions < 2 || numOfQuestions > 512)
                {
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add("Number of questions should be within 2 to 512.");
                    return (response, null);
                }

                // Get the number of available questions in db per type and total available
                int numOfAnimeImages = await _context.Images.Where(i => i.AnimeId != null).CountAsync();
                int numOfCharacterImages = await _context.Images.Where(i => i.CharacterVersionId != null).CountAsync();
                int numOfAnimeMusics = await _context.Musics.CountAsync();
                int totalAvailable = numOfAnimeImages + numOfCharacterImages + numOfAnimeMusics;

                if (numOfQuestions > totalAvailable)
                {
                    response.Status = ServiceStatus.BadRequest;
                    response.Messages.Add($"Number of questions requested exceeds the available number {totalAvailable}.");
                    return (response, null);
                }

                // Get the probability of each question type
                double animeImageProbability = numOfAnimeImages / totalAvailable;
                double characterImageProbability = numOfCharacterImages / totalAvailable;
                double animeMusicProbability = numOfAnimeMusics / totalAvailable;

                // Get the actual number of questions per type
                int numOfAnimeImageQuestions = (int)Math.Floor(numOfQuestions * animeImageProbability);
                int numOfCharacterImageQuestions = (int)Math.Floor(numOfQuestions * characterImageProbability);
                int numOfAnimeMusicQuestions = (int)Math.Floor(numOfQuestions * animeMusicProbability);
                int remainer = numOfQuestions - numOfAnimeImageQuestions - numOfCharacterImageQuestions - numOfAnimeMusicQuestions;

                // Handle remainer
                while (remainer != 0)
                {
                    if (numOfAnimeImageQuestions < numOfAnimeImages && remainer != 0)
                    {
                        numOfAnimeImageQuestions++;
                        remainer--;
                    }

                    if (numOfCharacterImageQuestions < numOfCharacterImages && remainer != 0)
                    {
                        numOfCharacterImageQuestions++;
                        remainer--;
                    }

                    if (numOfAnimeMusicQuestions < numOfAnimeMusics && remainer != 0)
                    {
                        numOfAnimeMusicQuestions++;
                        remainer--;
                    }
                }

                // Randomly select anime image questions & convert to Dto
                IEnumerable<ImageDto> animeImageQuestions = (await _context.Images
                        .Where(i => i.AnimeId != null)
                        .OrderBy(i => Guid.NewGuid())
                        .Take(numOfAnimeImageQuestions)
                        .Include(i => i.Anime)
                        .ToListAsync()
                    ).Select(ImageService.ToImageDto);

                foreach (ImageDto animeImageQuestion in animeImageQuestions)
                {
                    string answer = animeImageQuestion.AnimeDto!.AnimeName;

                    // Randomly select 3 wrong answers
                    List<string> choices = await _context.Animes
                        .Where(a => a.AnimeId != animeImageQuestion.AnimeDto!.AnimeId)
                        .OrderBy(a => Guid.NewGuid())
                        .Take(3)
                        .Select(a => a.AnimeName)
                        .ToListAsync();

                    choices.Add(answer);

                    Question question = new()
                    {
                        QuestionTitle = "Which anime this image comes from?",
                        ImagePath = animeImageQuestion?.ImagePath,
                        Answer = answer,
                        Choices = choices
                    };

                    animeQuizDto.Questions.Add(question);
                }

                // Randomly select character image question & convert to Dto
                IEnumerable<ImageDto> characterImageQuestions = (await _context.Images
                        .Where(i => i.CharacterVersionId != null)
                        .OrderBy(i => Guid.NewGuid())
                        .Take(numOfCharacterImageQuestions)
                        .Include(i => i.CharacterVersion)
                            .ThenInclude(cv => cv!.Character)
                        .Include(i => i.CharacterVersion)
                            .ThenInclude(cv => cv!.Animes)
                        .Include(i => i.CharacterVersion)
                            .ThenInclude(cv => cv!.VoiceActors)
                        .ToListAsync()
                    ).Select(ImageService.ToImageDto);

                foreach (ImageDto characterImageQuestion in characterImageQuestions)
                {
                    // Default CharacterName question available
                    List<CharacterImageQuestionType> availableTypes = [CharacterImageQuestionType.CharacterName];

                    // If this CharacterVersion has any VoiceActor
                    if (characterImageQuestion.CharacterVersionDto?.VoiceActorDtos?.Count > 0)
                    {
                        // Make VoiceActorName question available
                        availableTypes.Add(CharacterImageQuestionType.VoiceActorName);
                    }

                    // Randomly choose question type
                    CharacterImageQuestionType questionType = RandomChoice<CharacterImageQuestionType>(availableTypes);

                    switch (questionType)
                    {
                        case CharacterImageQuestionType.CharacterName:
                            string characterName = characterImageQuestion.CharacterVersionDto!.CharacterDto!.CharacterName;
                            string? versionName = characterImageQuestion.CharacterVersionDto.VersionName;

                            // Randomly choose anime name from this character version if any
                            string? animeName = RandomChoice<AnimeDto>(characterImageQuestion.CharacterVersionDto?.AnimeDtos)?.AnimeName;

                            string answer = characterName
                                + (versionName == null ? "" : $": {versionName}")
                                + (animeName == null ? "" : $", {animeName}");

                            // Randomly select 3 wrong answers
                            List<string> choices = await _context.CharacterVersions
                                .Where(cv => cv.CharacterVersionId != characterImageQuestion.CharacterVersionDto!.CharacterVersionId)
                                .OrderBy(cv => Guid.NewGuid())
                                .Take(3)
                                .Include(cv => cv.Character)
                                .Include(cv => cv.Animes)
                                .Select(cv => new
                                {
                                    CharacterName = cv.Character!.CharacterName,
                                    VersionName = cv.VersionName,
                                    AnimeName = cv.Animes!.OrderBy(a => Guid.NewGuid()).FirstOrDefault()!.AnimeName
                                })
                                .Select(c => c.CharacterName
                                    + (c.VersionName == null ? "" : $": {c.VersionName}")
                                    + (c.AnimeName == null ? "" : $", {c.AnimeName}")
                                )
                                .ToListAsync();

                            choices.Add(answer);

                            Question question = new()
                            {
                                QuestionTitle = "What is the name of this character?",
                                ImagePath = characterImageQuestion.ImagePath,
                                Answer = answer,
                                Choices = choices
                            };

                            animeQuizDto.Questions.Add(question);
                            break;
                        case CharacterImageQuestionType.VoiceActorName:
                            // Randomly choose a voice actor from this character version
                            answer = RandomChoice<StaffDto>(characterImageQuestion.CharacterVersionDto!.VoiceActorDtos)!.StaffName;

                            // Randomly select 3 wrong answers from staffs who have voice acted any character
                            choices = await _context.Staffs
                                .Include(s => s.VoiceActedCharacterVersions)
                                .Where(s => s.StaffName != answer && s.VoiceActedCharacterVersions!.Count != 0)
                                .OrderBy(s => Guid.NewGuid())
                                .Take(3)
                                .Select(s => s.StaffName)
                                .ToListAsync();

                            choices.Add(answer);

                            question = new()
                            {
                                QuestionTitle = "Who is the voice actor/actress of this character?",
                                ImagePath = characterImageQuestion.ImagePath,
                                Answer = answer,
                                Choices = choices
                            };

                            animeQuizDto.Questions.Add(question);
                            break;
                        default:
                            break;
                    }
                }

                // Randomly select anime music question & convert to Dto
                IEnumerable<MusicDto> animeMusicQuestions = (await _context.Musics
                    .OrderBy(m => Guid.NewGuid())
                    .Take(numOfAnimeMusicQuestions)
                    .Include(m => m.Anime)
                    .Include(m => m.Singers)
                    .ToListAsync()
                ).Select(MusicService.ToMusicDto);

                foreach (MusicDto animeMusicQuestion in animeMusicQuestions)
                {
                    // Default question type MusicName allowed
                    List<AnimeMusicQuestionType> availableTypes = [AnimeMusicQuestionType.MusicName];

                    // If this music has any singers
                    if (animeMusicQuestion.SingerDtos?.Count > 0)
                    {
                        // Make SingerName question type available
                        availableTypes.Add(AnimeMusicQuestionType.SingerName);
                    }

                    // Randomly choose a question type
                    AnimeMusicQuestionType questionType = RandomChoice<AnimeMusicQuestionType>(availableTypes);

                    switch (questionType)
                    {
                        case AnimeMusicQuestionType.MusicName:
                            string musicName = animeMusicQuestion.MusicName;
                            string animeName = animeMusicQuestion.AnimeDto!.AnimeName;
                            string answer = musicName + ", " + animeName;

                            // Randomly select 3 wrong answers
                            List<string> choices = await _context.Musics
                                .Where(m => m.MusicId != animeMusicQuestion.MusicId)
                                .OrderBy(m => Guid.NewGuid())
                                .Take(3)
                                .Include(m => m.Anime)
                                .Select(m => m.MusicName + ", " + m.Anime!.AnimeName)
                                .ToListAsync();

                            choices.Add(answer);

                            Question question = new()
                            {
                                QuestionTitle = "What is the name of this music?",
                                MusicPath = animeMusicQuestion.MusicPath,
                                Answer = answer,
                                Choices = choices
                            };

                            animeQuizDto.Questions.Add(question);
                            break;
                        case AnimeMusicQuestionType.SingerName:
                            // Randomly select 1 singer from this music
                            answer = RandomChoice<StaffDto>(animeMusicQuestion.SingerDtos)!.StaffName;

                            // Randomly select 3 wrong answers from staffs who have sang any music
                            choices = await _context.Staffs
                                .Include(s => s.SungMusics)
                                .Where(s => s.StaffName != answer && s.SungMusics!.Count != 0)
                                .OrderBy(s => Guid.NewGuid())
                                .Take(3)
                                .Select(s => s.StaffName)
                                .ToListAsync();

                            choices.Add(answer);

                            question = new()
                            {
                                QuestionTitle = "Who is the singer of this music?",
                                MusicPath = animeMusicQuestion.MusicPath,
                                Answer = answer,
                                Choices = choices
                            };

                            animeQuizDto.Questions.Add(question);
                            break;
                        default:
                            break;
                    }
                }

                // Randomly shuffle the answers
                foreach (Question question in animeQuizDto.Questions)
                {
                    _random.Shuffle<string>(CollectionsMarshal.AsSpan(question.Choices));
                }

                // Randomly shuffle the questions
                _random.Shuffle<Question>(CollectionsMarshal.AsSpan(animeQuizDto.Questions));
                response.Status = ServiceStatus.Ok;
                return (response, animeQuizDto);
            }
            catch (Exception ex)
            {
                response.Status = ServiceStatus.Error;
                response.Messages.Add("Unknown error when generating anime quiz.");
                response.Messages.Add(ex.Message);
                return (response, null);
            }
        }
    }
}
