

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using QuizGame.Models;

using QuizGame.Services;

using QuizGame.Repositories.Interfaces;

using System.Security.Claims;


namespace QuizGame.Controllers
{
    /// <summary>
    /// Test controller for verifying LeaderboardService and ChatService functionality.
    /// This controller is for development/testing purposes only.
    /// </summary>
    public class TestController : Controller
    {

        private readonly LeaderboardService _leaderboardService;

        private readonly ChatService _chatService;

        private readonly IChatMessageRepository _chatMessageRepository;

        private readonly IGameRepository _gameRepository;
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly IGameQuestionRepository _gameQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;

        private readonly UserManager<User> _userManager;



        public TestController(

            LeaderboardService leaderboardService,

            ChatService chatService,

            IChatMessageRepository chatMessageRepository,

            IGameRepository gameRepository,
            IGamePlayerRepository gamePlayerRepository,
            IGameQuestionRepository gameQuestionRepository,
            IQuestionRepository questionRepository,
            IPlayerAnswerRepository playerAnswerRepository,

            UserManager<User> userManager)

        {

            _leaderboardService = leaderboardService;

            _chatService = chatService;

            _chatMessageRepository = chatMessageRepository;
            _gameRepository = gameRepository;
            _gamePlayerRepository = gamePlayerRepository;
            _gameQuestionRepository = gameQuestionRepository;
            _questionRepository = questionRepository;
            _playerAnswerRepository = playerAnswerRepository;

            _userManager = userManager;

        }


        /// <summary>
        /// Test page showing all test endpoints and current status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tests = new List<TestInfo>
            {
                new TestInfo
                {
                    Name = "Chat Service - Save Message",
                    Description = "Saves a test chat message to game ID 1 (if exists)",
                    Url = "/test/chat/save",
                    Method = "POST",
                    RequiresAuth = true
                },
                new TestInfo
                {
                    Name = "Chat Service - Get History",
                    Description = "Retrieves chat history for game ID 1",
                    Url = "/test/chat/history",
                    Method = "GET",
                    RequiresAuth = false
                },
                new TestInfo
                {
                    Name = "Leaderboard - Global Top 10",
                    Description = "Shows top 10 players by total score",
                    Url = "/test/leaderboard/global",
                    Method = "GET",
                    RequiresAuth = false
                },
                new TestInfo
                {
                    Name = "Leaderboard - Game Results",
                    Description = "Shows results for a specific room code (requires roomCode parameter)",
                    Url = "/test/leaderboard/game?roomCode=XXXXX",
                    Method = "GET",
                    RequiresAuth = false
                },
                new TestInfo
                {
                    Name = "Game - Summary View",
                    Description = "Shows game summary page for a specific room code (requires roomCode parameter)",
                    Url = "/test/game/summary?roomCode=XXXXX",
                    Method = "GET",
                    RequiresAuth = false
                },
                new TestInfo
                {
                    Name = "Leaderboard - Current Standings",
                    Description = "Shows current standings for a room (requires roomCode parameter)",
                    Url = "/test/leaderboard/standings?roomCode=XXXXX",
                    Method = "GET",
                    RequiresAuth = false
                },
                new TestInfo
                {
                    Name = "Leaderboard - Finalize Game",
                    Description = "Finalizes game ID 1 (adds scores to user totals)",
                    Url = "/test/leaderboard/finalize/1",
                    Method = "POST",
                    RequiresAuth = true
                },
                new TestInfo
                {
                    Name = "Create Test Data",
                    Description = "Creates a test chat message and test game data",
                    Url = "/test/create-data",
                    Method = "POST",
                    RequiresAuth = true
                },
                new TestInfo
                {
                    Name = "Clear Test Data",
                    Description = "Removes test chat messages for game ID 1",
                    Url = "/test/clear-data",
                    Method = "POST",
                    RequiresAuth = true
                },
                new TestInfo
                {
                    Name = "Create Full Scenario",
                    Description = "Creates a complete game with players, questions, answers, and chat messages",
                    Url = "/test/create-full-scenario",
                    Method = "POST",
                    RequiresAuth = true
                }
            };

            return View(tests);
        }

        #region Chat Service Tests

        /// <summary>
        /// Test: Save a chat message
        /// POST /test/chat/save
        /// </summary>
        [HttpPost]
        [Route("test/chat/save")]
        public async Task<IActionResult> SaveChatMessage(int gameId = 1, string text = "Test message")
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : "test-user-id";

                var message = _chatService.SaveAsync(gameId, userId, text);

                return Json(new
                {
                    success = true,
                    message = "Chat message saved successfully",
                    data = new
                    {
                        message.Id,
                        message.GameId,
                        message.UserId,
                        message.Text,
                        message.SentAt
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Test: Get chat history
        /// GET /test/chat/history?gameId=1
        /// </summary>
        [HttpGet]
        [Route("test/chat/history")]
        public IActionResult GetChatHistory(int gameId = 1)
        {
            try
            {
                var messages = _chatService.GetHistoryAsync(gameId).ToList();

                return Json(new
                {
                    success = true,
                    message = $"Retrieved {messages.Count} messages",
                    data = messages.Select(m => new
                    {
                        m.Id,
                        m.GameId,
                        m.UserId,
                        m.Text,
                        m.SentAt,
                        UserName = m.User?.UserName ?? "Unknown"
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        #endregion

        #region Leaderboard Service Tests


        /// <summary>

        /// Test: Get global top players

        /// GET /test/leaderboard/global?count=10

        /// </summary>

        [HttpGet]

        [Route("test/leaderboard/global")]

        public async Task<IActionResult> GetGlobalTop(int count = 10)

        {

            try

            {

                // Force fresh data by reloading users from database

                var users = await _userManager.Users
                    .OrderByDescending(u => u.TotalScore)
                    .Take(count)
                    .ToListAsync();

                return Json(new

                {

                    success = true,

                    message = $"Retrieved top {users.Count} players",

                    data = users.Select(u => new

                    {

                        u.Id,

                        u.UserName,

                        u.Email,

                        u.TotalScore

                    })

                });

            }

            catch (Exception ex)

            {

                return Json(new

                {

                    success = false,

                    message = $"Error: {ex.Message}",

                    data = (object?)null

                });

            }

        }


        /// <summary>
        /// Test: Get game results by room code
        /// GET /test/leaderboard/game?roomCode=XXXXX
        /// </summary>
        [HttpGet]
        [Route("test/leaderboard/game")]
        public async Task<IActionResult> GetGameResults(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return Json(new
                {
                    success = false,
                    message = "roomCode parameter is required",
                    data = (object?)null
                });
            }

            try
            {
                var results = await _leaderboardService.GetGameResultsAsync(roomCode);

                return Json(new
                {
                    success = true,
                    message = $"Retrieved results for room {results.RoomCode}",
                    data = new
                    {
                        results.RoomCode,
                        results.CategoryName,
                        FinalRankings = results.FinalRankings,
                        QuestionResults = results.QuestionResults
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Test: Get current standings for a game
        /// GET /test/leaderboard/standings?roomCode=XXXXX
        /// </summary>
        [HttpGet]
        [Route("test/leaderboard/standings")]
        public async Task<IActionResult> GetCurrentStandings(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return Json(new
                {
                    success = false,
                    message = "roomCode parameter is required",
                    data = (object?)null
                });
            }

            try
            {
                var standings = await _leaderboardService.GetCurrentStandingsAsync(roomCode);

                return Json(new
                {
                    success = true,
                    message = $"Retrieved {standings.Count} players",
                    data = standings
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }


        /// <summary>

        /// Test: Finalize a game

        /// POST /test/leaderboard/finalize/{gameId}

        /// </summary>

        [HttpPost]

        [Route("test/leaderboard/finalize/{gameId}")]

        public async Task<IActionResult> FinalizeGame(int gameId)

        {

            try

            {

                // Get players before finalization to report what was updated
                var gamePlayers = _gamePlayerRepository.GetByGame(gameId).ToList();
                var updates = new List<object>();

                foreach (var gp in gamePlayers)
                {
                    var user = await _userManager.FindByIdAsync(gp.UserId);
                    if (user != null)
                    {
                        var oldScore = user.TotalScore;
                        user.TotalScore += gp.Score;
                        await _userManager.UpdateAsync(user);

                        updates.Add(new

                        {

                            UserName = user.UserName,

                            GameScore = gp.Score,
                            OldTotalScore = oldScore,
                            NewTotalScore = user.TotalScore
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Game {gameId} finalized. Updated {updates.Count} player(s).",
                    data = new
                    {
                        GameId = gameId,
                        Updates = updates
                    }
                });

            }

            catch (Exception ex)

            {

                return Json(new

                {

                    success = false,

                    message = $"Error: {ex.Message}",

                    data = (object?)null

                });

            }

        }


        #endregion

        #region Game Controller Tests

        /// <summary>
        /// Test: Get game summary view
        /// GET /test/game/summary?roomCode=XXXXX
        /// </summary>
        [HttpGet]
        [Route("test/game/summary")]
        public async Task<IActionResult> GetGameSummary(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return Json(new
                {
                    success = false,
                    message = "roomCode parameter is required",
                    data = (object?)null
                });
            }

            try
            {
                var results = await _leaderboardService.GetGameResultsAsync(roomCode);

                return Json(new
                {
                    success = true,
                    message = $"Retrieved summary for room {results.RoomCode}",
                    data = new
                    {
                        results.RoomCode,
                        results.CategoryName,
                        TotalPlayers = results.FinalRankings.Count,
                        TotalQuestions = results.QuestionResults.Count,
                        FinalRankings = results.FinalRankings,
                        QuestionResults = results.QuestionResults
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        #endregion

        #region Data Setup Tests

        /// <summary>
        /// Test: Create test data (chat messages, ensure game exists)
        /// POST /test/create-data
        /// </summary>
        [HttpPost]
        [Route("test/create-data")]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {


                // Get current logged-in user

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))

                {

                    return Json(new

                    {
                        success = false,

                        message = "User must be logged in to create test data.",
                        data = (object?)null

                    });

                }


                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,

                        message = "Current user not found in database.",
                        data = (object?)null

                    });
                }

                // Get at least one other user for multiplayer testing

                var allUsers = _userManager.Users.Where(u => u.Id != currentUserId).Take(3).ToList();

                var users = new List<User> { currentUser };
                users.AddRange(allUsers);


                if (users.Count < 2)
                {
                    return Json(new
                    {
                        success = false,

                        message = "Need at least 2 users in database for multiplayer testing. Please register another user.",
                        data = (object?)null

                    });
                }


                // Find or create a game with ID 1 (or use first available game)
                var game = _gameRepository.Get(1);
                if (game == null)
                {
                    var allGames = _gameRepository.GetAll().FirstOrDefault();
                    if (allGames == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No games found in database. Create a game first via Lobby.",
                            data = (object?)null
                        });
                    }
                    game = allGames;
                }

                // Create test chat messages
                var testMessages = new[]
                {
                    "Hello everyone!",
                    "Good luck to all players!",
                    "This is a test message",
                    "Ready for the quiz?",
                    "Let's go!"
                };


                // Create chat messages as the CURRENT user (not always users[0])
                foreach (var msg in testMessages)

                {

                    _chatService.SaveAsync(game.Id, currentUser.Id, msg);
                }




                // Ensure game has multiple players with scores for leaderboard testing

                // Always try to have at least 2-3 players in the game


                var existingPlayers = _gamePlayerRepository.GetByGame(game.Id).ToList();

                var playerUserIds = existingPlayers.Select(p => p.UserId).ToHashSet();

                // Add current user if not already in game (should already be there as host)
                if (!playerUserIds.Contains(currentUser.Id))
                {
                    _gamePlayerRepository.Add(new GamePlayer

                    {

                        GameId = game.Id,

                        UserId = currentUser.Id,

                        Score = new Random().Next(50, 200)
                    });

                    _gamePlayerRepository.Save();

                    playerUserIds.Add(currentUser.Id);
                }

                // Add other users to ensure multiplayer (at least 2 total players)

                var usersToAdd = users.Where(u => !playerUserIds.Contains(u.Id)).Take(2).ToList();
                foreach (var userToAdd in usersToAdd)
                {

                    _gamePlayerRepository.Add(new GamePlayer

                    {

                        GameId = game.Id,

                        UserId = userToAdd.Id,

                        Score = new Random().Next(50, 200)

                    });

                }

                _gamePlayerRepository.Save();



                // Update any players with zero scores to have random scores
                var allGamePlayers = _gamePlayerRepository.GetByGame(game.Id).ToList();

                foreach (var player in allGamePlayers)

                {

                    if (player.Score == 0)

                    {

                        player.Score = new Random().Next(50, 200);

                        _gamePlayerRepository.Update(player);

                    }

                }

                _gamePlayerRepository.Save();




                // Get final player count for response
                var finalPlayers = _gamePlayerRepository.GetByGame(game.Id).Count();
                var currentUserScore = _gamePlayerRepository.GetByGameAndUser(game.Id, currentUser.Id)?.Score ?? 0;

                return Json(new

                {

                    success = true,

                    message = $"Test data created for game ID {game.Id}",

                    data = new

                    {

                        GameId = game.Id,

                        RoomCode = game.RoomCode,

                        MessagesCreated = testMessages.Length,

                        CurrentUser = currentUser.UserName,

                        CurrentUserId = currentUser.Id,
                        CurrentUserScore = currentUserScore,

                        TotalPlayersInGame = finalPlayers,
                        UsersAvailable = _userManager.Users.Count()
                    }

                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Test: Clear test chat messages for game ID 1
        /// POST /test/clear-data
        /// </summary>

        [HttpPost]

        [Route("test/clear-data")]

        public IActionResult ClearTestData(int gameId = 1)

        {

            try

            {

                // Get all chat messages for the game

                var messages = _chatMessageRepository.GetByGame(gameId).ToList();



                foreach (var message in messages)

                {

                    _chatMessageRepository.Delete(message.Id);
                }


                if (messages.Any())
                {
                    _chatMessageRepository.Save();
                }

                return Json(new
                {

                    success = true,
                    message = $"Cleared {messages.Count} chat messages for game ID {gameId}",

                    data = (object?)null

                });

            }

            catch (Exception ex)

            {

                return Json(new

                {

                    success = false,

                    message = $"Error: {ex.Message}",

                    data = (object?)null

                });

            }

        }

        /// <summary>
        /// Test: Create full game scenario with players, questions, answers, and chat
        /// POST /test/create-full-scenario
        /// </summary>
        [HttpPost]
        [Route("test/create-full-scenario")]
        public async Task<IActionResult> CreateFullScenario()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "User must be logged in.",
                        data = (object?)null
                    });
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Current user not found.",
                        data = (object?)null
                    });
                }

                var allUsers = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
                if (allUsers.Count < 2)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Need at least 2 additional users. Please register more users.",
                        data = (object?)null
                    });
                }

                var category = _questionRepository.GetAll()
                    .Include(q => q.Category)
                    .FirstOrDefault()?.Category;

                var roomCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
                var game = new Game
                {
                    RoomCode = roomCode,
                    HostId = currentUserId,
                    CategoryId = category?.Id,
                    QuestionCount = 3,
                    Status = GameStatus.Finished
                };
                _gameRepository.Add(game);
                _gameRepository.Save();

                var users = new List<User> { currentUser };
                users.AddRange(allUsers.Take(2));

                var random = new Random();
                var playerData = new List<object>();
                var totalScore1 = 0;

                for (int i = 0; i < users.Count; i++)
                {
                    var score = (i == 0) ? 350 : random.Next(100, 300);
                    if (i == 0) totalScore1 = score;

                    var gamePlayer = new GamePlayer
                    {
                        GameId = game.Id,
                        UserId = users[i].Id,
                        Score = score
                    };
                    _gamePlayerRepository.Add(gamePlayer);
                    playerData.Add(new { UserName = users[i].UserName, Score = score });
                }
                _gamePlayerRepository.Save();

                var questions = _questionRepository.GetAll()
                    .Include(q => q.AnswerOptions)
                    .Take(3)
                    .ToList();
                var questionResults = new List<object>();
                var answerIndex = 0;

                foreach (var q in questions)
                {
                    var gameQuestion = new GameQuestion
                    {
                        GameId = game.Id,
                        QuestionId = q.Id,
                        Order = questions.IndexOf(q) + 1,
                        Status = QuestionStatus.Closed,
                        WinnerId = currentUserId,
                        PointsAwarded = q.Points
                    };
                    _gameQuestionRepository.Add(gameQuestion);
                    _gameQuestionRepository.Save();

                    var correctAnswer = q.AnswerOptions.FirstOrDefault(a => a.IsCorrect);

                    foreach (var user in users)
                    {
                        var isWinner = user.Id == currentUserId;
                        var answerOptionId = isWinner 
                            ? correctAnswer?.Id ?? 1 
                            : q.AnswerOptions.ElementAt(random.Next(q.AnswerOptions.Count)).Id;

                        var playerAnswer = new PlayerAnswer
                        {
                            GameQuestionId = gameQuestion.Id,
                            UserId = user.Id,
                            AnswerOptionId = answerOptionId,
                            SubmittedAt = DateTime.UtcNow.AddSeconds(random.Next(5, 15)),
                            IsWinningAnswer = isWinner
                        };
                        _playerAnswerRepository.Add(playerAnswer);
                    }
                    _playerAnswerRepository.Save();

                    questionResults.Add(new
                    {
                        Question = q.Text,
                        Winner = currentUser.UserName,
                        Points = q.Points,
                        CorrectAnswer = correctAnswer?.Text
                    });
                }

                var chatMessages = new[] { "Good game!", "That was fun!", "Nice playing with you!" };
                foreach (var msg in chatMessages)
                {
                    var user = users[random.Next(users.Count)];
                    _chatMessageRepository.Add(new ChatMessage
                    {
                        GameId = game.Id,
                        UserId = user.Id,
                        Text = msg,
                        SentAt = DateTime.UtcNow
                    });
                }
                _chatMessageRepository.Save();

                return Json(new
                {
                    success = true,
                    message = $"Full scenario created for room {roomCode}",
                    data = new
                    {
                        GameId = game.Id,
                        RoomCode = roomCode,
                        Status = game.Status.ToString(),
                        Category = category?.Name ?? "All",
                        Players = playerData,
                        Questions = questionResults,
                        ChatMessages = chatMessages.Length
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    data = (object?)null
                });
            }
        }


        #endregion
    }

    /// <summary>
    /// Helper class for test information
    /// </summary>
    public class TestInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public bool RequiresAuth { get; set; }
    }
}
