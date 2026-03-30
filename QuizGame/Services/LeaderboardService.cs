using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using QuizGame.Models;

using QuizGame.Repositories.Interfaces;
using QuizGame.ViewModels;


namespace QuizGame.Services
{
   
    public class LeaderboardService
    {
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly IGameQuestionRepository _gameQuestionRepository;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<User> _userManager;

        public LeaderboardService(
            IGamePlayerRepository gamePlayerRepository,
            IGameQuestionRepository gameQuestionRepository,
            IPlayerAnswerRepository playerAnswerRepository,
            IGameRepository gameRepository,
            UserManager<User> userManager)
        {
            _gamePlayerRepository = gamePlayerRepository;
            _gameQuestionRepository = gameQuestionRepository;
            _playerAnswerRepository = playerAnswerRepository;
            _gameRepository = gameRepository;
            _userManager = userManager;
        }

  
        public async Task<List<User>> GetGlobalTopAsync(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException("Number of users must be positive");
            }

            await SyncTotalScoresFromFinishedGamesAsync();

            return await _userManager.Users
                .OrderByDescending(u => u.TotalScore)
                .Take(n)
                .ToListAsync();
        }

        private async Task SyncTotalScoresFromFinishedGamesAsync()
        {
            var finishedGameIds = await _gameRepository.GetAll()
                .Where(g => g.Status == GameStatus.Finished)
                .Select(g => g.Id)
                .ToListAsync();

            var scoreByUser = finishedGameIds.Count == 0
                ? new Dictionary<string, int>()
                : await _gamePlayerRepository.GetAll()
                    .Where(gp => finishedGameIds.Contains(gp.GameId))
                    .GroupBy(gp => gp.UserId)
                    .Select(g => new { UserId = g.Key, TotalScore = g.Sum(x => x.Score) })
                    .ToDictionaryAsync(x => x.UserId, x => x.TotalScore);

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var expectedScore = scoreByUser.TryGetValue(user.Id, out var score) ? score : 0;
                if (user.TotalScore != expectedScore)
                {
                    user.TotalScore = expectedScore;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        public async Task<GameResultViewModel> GetGameResultsAsync(string roomCode)

        {

            if (string.IsNullOrWhiteSpace(roomCode))
            {
                throw new ArgumentException("Room code cannot be empty");
            }


            var game = _gameRepository.GetByRoomCode(roomCode);

            if (game == null)
            {
                throw new InvalidOperationException($"Game with room code '{roomCode}' not found");
            }


            var gamePlayers = _gamePlayerRepository.GetByGame(game.Id).ToList();



            var userIds = gamePlayers.Select(gp => gp.UserId).Distinct().ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var gp in gamePlayers)
            {
                if (users.TryGetValue(gp.UserId, out var user))
                {
                    gp.User = user;
                }
            }


            var rankedPlayers = gamePlayers
                .OrderByDescending(gp => gp.Score)
                .ThenBy(gp => gp.User?.UserName)
                .ToList();




            var gameQuestions = _gameQuestionRepository.GetAll()
                .Where(gq => gq.GameId == game.Id)
                .Include(gq => gq.Question)
                    .ThenInclude(q => q.AnswerOptions)
                .OrderBy(gq => gq.Order)
                .ToList();




            var questionResults = new List<QuestionResultViewModel>();

            foreach (var gq in gameQuestions)
            {
                var question = gq.Question;

                if (question == null)
                {
                    continue;
                }

                string? winnerName = null;

                if (gq.WinnerId != null && users.TryGetValue(gq.WinnerId, out var winnerUser))
                {
                    winnerName = winnerUser.UserName;
                }



                var correctAnswer = question.AnswerOptions.FirstOrDefault(ao => ao.IsCorrect)?.Text ?? "Unknown";


                questionResults.Add(new QuestionResultViewModel
                {
                    QuestionText = question.Text,
                    Order = gq.Order,
                    WinnerName = winnerName,
                    PointsAwarded = gq.PointsAwarded,
                    CorrectAnswer = correctAnswer
                });

            }



            return new GameResultViewModel
            {
                RoomCode = game.RoomCode,
                CategoryName = game.Category?.Name ?? "All Categories",
                FinalRankings = rankedPlayers.Select(rp => new PlayerRankingViewModel
                {
                    UserName = rp.User?.UserName ?? "Unknown",
                    Score = rp.Score,
                    Rank = rankedPlayers.IndexOf(rp) + 1
                }).ToList(),
                QuestionResults = questionResults
            };

        }


        public async Task FinalizeGameAsync(int gameId)
        {
            if (gameId <= 0)
            {
                throw new ArgumentException("Invalid game ID");
            }

            var gamePlayers = _gamePlayerRepository.GetByGame(gameId).ToList();

            foreach (var gamePlayer in gamePlayers)
            {
                var user = await _userManager.FindByIdAsync(gamePlayer.UserId);

                if (user != null)
                {
                    user.TotalScore += gamePlayer.Score;
                    await _userManager.UpdateAsync(user);
                }

            }
        }

       
        public async Task<List<PlayerRankingViewModel>> GetCurrentStandingsAsync(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                throw new ArgumentException("Room code cannot be empty");
            }

            var game = _gameRepository.GetByRoomCode(roomCode);

            if (game == null)
            {
                throw new InvalidOperationException($"Game with room code '{roomCode}' not found");
            }

            var gamePlayers = _gamePlayerRepository.GetByGame(game.Id).ToList();

            foreach (var gp in gamePlayers)
            {
                if (gp.User == null)
                {
                    gp.User = await _userManager.FindByIdAsync(gp.UserId);
                }
            }

            var sortedPlayers = gamePlayers
            .OrderByDescending(gp => gp.Score)
            .ThenBy(gp => gp.User?.UserName)
            .ToList();

            return sortedPlayers
                .Select((gp, index) => new PlayerRankingViewModel
                {
                    UserName = gp.User?.UserName ?? "Unknown",
                    Score = gp.Score,
                    Rank = index + 1
                })
                .ToList();


        }
    }
}
