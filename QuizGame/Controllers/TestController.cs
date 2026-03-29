using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizGame.Models;
using QuizGame.Services;
using QuizGame.Repositories.Interfaces;
using System.Security.Claims;

namespace QuizGame.Controllers;

public class TestController : Controller
{
    private readonly LeaderboardService _leaderboardService;
    private readonly ChatService _chatService;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository;
    private readonly UserManager<User> _userManager;

    public TestController(
        LeaderboardService leaderboardService,
        ChatService chatService,
        IChatMessageRepository chatMessageRepository,
        IGameRepository gameRepository,
        IGamePlayerRepository gamePlayerRepository,
        UserManager<User> userManager)
    {
        _leaderboardService = leaderboardService;
        _chatService = chatService;
        _chatMessageRepository = chatMessageRepository;
        _gameRepository = gameRepository;
        _gamePlayerRepository = gamePlayerRepository;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var tests = new List<TestInfo>
        {
            new() { Name = "Chat - Save Message",        Url = "/test/chat/save",                     Method = "POST", RequiresAuth = true,  Description = "Saves a test chat message to game ID 1" },
            new() { Name = "Chat - Get History",         Url = "/test/chat/history",                  Method = "GET",  RequiresAuth = false, Description = "Retrieves chat history for game ID 1" },
            new() { Name = "Leaderboard - Global Top 10",Url = "/test/leaderboard/global",            Method = "GET",  RequiresAuth = false, Description = "Shows top 10 players by total score" },
            new() { Name = "Leaderboard - Game Results", Url = "/test/leaderboard/game?roomCode=XXXX",Method = "GET",  RequiresAuth = false, Description = "Shows results for a room code" },
            new() { Name = "Leaderboard - Standings",    Url = "/test/leaderboard/standings?roomCode=XXXX", Method = "GET", RequiresAuth = false, Description = "Current standings for a room" },
            new() { Name = "Leaderboard - Finalize",     Url = "/test/leaderboard/finalize/1",        Method = "POST", RequiresAuth = true,  Description = "Finalizes game ID 1" },
            new() { Name = "Create Test Data",           Url = "/test/create-data",                   Method = "POST", RequiresAuth = true,  Description = "Creates test data" },
            new() { Name = "Clear Test Data",            Url = "/test/clear-data",                    Method = "POST", RequiresAuth = false, Description = "Clears chat messages for game ID 1" },
        };
        return View(tests);
    }

    // POST /test/chat/save
    [HttpPost, Route("test/chat/save")]
    public IActionResult SaveChatMessage(int gameId = 1, string text = "Test message")
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : "test-user-id";

            var message = _chatService.SaveAsync(gameId, userId!, text);

            return Json(new { success = true, message = "Chat message saved", data = new { message.Id, message.GameId, message.UserId, message.Text, message.SentAt } });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // GET /test/chat/history?gameId=1
    [HttpGet, Route("test/chat/history")]
    public IActionResult GetChatHistory(int gameId = 1)
    {
        try
        {
            // Their sync GetByGame — no await needed
            var messages = _chatMessageRepository.GetByGame(gameId).ToList();
            return Json(new
            {
                success = true,
                message = $"Retrieved {messages.Count} messages",
                data = messages.Select(m => new { m.Id, m.GameId, m.UserId, m.Text, m.SentAt, UserName = m.User?.UserName ?? "Unknown" })
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // GET /test/leaderboard/global?count=10
    [HttpGet, Route("test/leaderboard/global")]
    public async Task<IActionResult> GetGlobalTop(int count = 10)
    {
        try
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.TotalScore)
                .Take(count)
                .ToListAsync();

            return Json(new { success = true, message = $"Top {users.Count} players", data = users.Select(u => new { u.Id, u.UserName, u.Email, u.TotalScore }) });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // GET /test/leaderboard/game?roomCode=XXXXX
    [HttpGet, Route("test/leaderboard/game")]
    public async Task<IActionResult> GetGameResults(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            return Json(new { success = false, message = "roomCode required", data = (object?)null });
        try
        {
            var results = await _leaderboardService.GetGameResultsAsync(roomCode);
            return Json(new { success = true, data = new { results.RoomCode, results.CategoryName, results.FinalRankings, results.QuestionResults } });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // GET /test/leaderboard/standings?roomCode=XXXXX
    [HttpGet, Route("test/leaderboard/standings")]
    public async Task<IActionResult> GetCurrentStandings(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            return Json(new { success = false, message = "roomCode required", data = (object?)null });
        try
        {
            var standings = await _leaderboardService.GetCurrentStandingsAsync(roomCode);
            return Json(new { success = true, message = $"{standings.Count} players", data = standings });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // POST /test/leaderboard/finalize/{gameId}
    [HttpPost, Route("test/leaderboard/finalize/{gameId}")]
    public async Task<IActionResult> FinalizeGame(int gameId)
    {
        try
        {
            // Their sync GetByGame — IQueryable, call ToList()
            var gamePlayers = _gamePlayerRepository.GetByGame(gameId).ToList();
            var updates = new List<object>();

            foreach (var gp in gamePlayers)
            {
                var user = await _userManager.FindByIdAsync(gp.UserId);
                if (user != null)
                {
                    var old = user.TotalScore;
                    user.TotalScore += gp.Score;
                    await _userManager.UpdateAsync(user);
                    updates.Add(new { UserName = user.UserName, GameScore = gp.Score, OldTotalScore = old, NewTotalScore = user.TotalScore });
                }
            }

            return Json(new { success = true, message = $"Finalized. Updated {updates.Count} player(s).", data = new { GameId = gameId, Updates = updates } });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // POST /test/create-data
    [HttpPost, Route("test/create-data")]
    public async Task<IActionResult> CreateTestData()
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Json(new { success = false, message = "Must be logged in.", data = (object?)null });

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Json(new { success = false, message = "User not found.", data = (object?)null });

            var otherUsers = _userManager.Users.Where(u => u.Id != currentUserId).Take(3).ToList();
            var users = new List<User> { currentUser };
            users.AddRange(otherUsers);

            if (users.Count < 2)
                return Json(new { success = false, message = "Need at least 2 users. Register another user first.", data = (object?)null });

            // Their sync Get(int id) from base Repository<Game>
            var game = _gameRepository.Get(1) ?? _gameRepository.GetAll().FirstOrDefault();
            if (game == null)
                return Json(new { success = false, message = "No games found. Create a game via Lobby first.", data = (object?)null });

            var testMessages = new[] { "Hello everyone!", "Good luck!", "This is a test message", "Ready for the quiz?", "Let's go!" };
            foreach (var msg in testMessages)
                _chatService.SaveAsync(game.Id, currentUser.Id, msg);

            // Their sync GetByGame returns IQueryable — call ToList()
            var existingPlayers = _gamePlayerRepository.GetByGame(game.Id).ToList();
            var playerIds = existingPlayers.Select(p => p.UserId).ToHashSet();

            if (!playerIds.Contains(currentUser.Id))
            {
                _gamePlayerRepository.Add(new GamePlayer { GameId = game.Id, UserId = currentUser.Id, Score = new Random().Next(50, 200) });
                _gamePlayerRepository.Save();
                playerIds.Add(currentUser.Id);
            }

            foreach (var u in users.Where(u => !playerIds.Contains(u.Id)).Take(2))
                _gamePlayerRepository.Add(new GamePlayer { GameId = game.Id, UserId = u.Id, Score = new Random().Next(50, 200) });

            _gamePlayerRepository.Save();

            // Update zero-score players
            foreach (var p in _gamePlayerRepository.GetByGame(game.Id).Where(p => p.Score == 0).ToList())
            {
                p.Score = new Random().Next(50, 200);
                _gamePlayerRepository.Update(p);
            }
            _gamePlayerRepository.Save();

            var finalCount = _gamePlayerRepository.GetByGame(game.Id).Count();
            var myScore = _gamePlayerRepository.GetByGameAndUser(game.Id, currentUser.Id)?.Score ?? 0;

            return Json(new
            {
                success = true,
                message = $"Test data created for game {game.Id}",
                data = new { GameId = game.Id, RoomCode = game.RoomCode, MessagesCreated = testMessages.Length, CurrentUser = currentUser.UserName, CurrentUserScore = myScore, TotalPlayersInGame = finalCount }
            });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }

    // POST /test/clear-data
    [HttpPost, Route("test/clear-data")]
    public IActionResult ClearTestData(int gameId = 1)
    {
        try
        {
            // Their sync GetByGame on IChatMessageRepository
            var messages = _chatMessageRepository.GetByGame(gameId).ToList();
            foreach (var m in messages)
                _chatMessageRepository.Delete(m.Id);

            if (messages.Any())
                _chatMessageRepository.Save();

            return Json(new { success = true, message = $"Cleared {messages.Count} messages for game {gameId}", data = (object?)null });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message, data = (object?)null }); }
    }
}

public class TestInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public bool RequiresAuth { get; set; }
}