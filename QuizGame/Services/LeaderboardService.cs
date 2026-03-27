using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using QuizGame.Models;

using QuizGame.Repositories.Interfaces;
using QuizGame.ViewModels;


namespace QuizGame.Services
{
    /// <summary>
    /// Service for managing leaderboards, game results, and user scoring
    /// </summary>
    public class LeaderboardService
    {
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly IGameQuestionRepository _gameQuestionRepository;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the LeaderboardService
        /// </summary>
        /// <param name="gamePlayerRepository">Repository for game player data</param>
        /// <param name="gameQuestionRepository">Repository for game questions data</param>
        /// <param name="playerAnswerRepository">Repository for player answers data</param>
        /// <param name="gameRepository">Repository for game data</param>
        /// <param name="userManager">ASP.NET Identity user manager</param>
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

        /// <summary>
        /// Gets the top N users by total accumulated score across all games
        /// </summary>
        /// <param name="n">Number of top users to retrieve</param>
        /// <returns>List of top users ordered by TotalScore descending</returns>
        public async Task<List<User>> GetGlobalTopAsync(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException("Number of users must be positive");
            }
            return await _userManager.Users
                .OrderByDescending(u => u.TotalScore)
                .Take(n)
                .ToListAsync();
        }


        /// <summary>
        /// Gets detailed results for a specific game, including final scores and per-question breakdown
        /// </summary>
        /// <param name="roomCode">The room code of the game</param>
        /// <returns>Game result view model with player rankings and question details</returns>
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
                .OrderBy(gq => gq.Order)
                .ToList();




            var questionResults = new List<QuestionResultViewModel>();

            foreach (var gq in gameQuestions)
            {

                var question = _gameQuestionRepository.Get(gq.Id)?.Question;

                if (question == null) continue;

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


        /// <summary>
        /// Finalizes a game by updating each player's total accumulated score
        /// </summary>
        /// <param name="gameId">The ID of the game to finalize</param>
        /// <returns>Task representing the asynchronous operation</returns>
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

        /// <summary>
        /// Gets the current leaderboard standings for an in-progress game
        /// </summary>
        /// <param name="roomCode">The room code of the game</param>
        /// <returns>List of current player rankings ordered by score descending</returns>
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
