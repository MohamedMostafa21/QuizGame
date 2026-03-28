using Microsoft.AspNetCore.Mvc;
using QuizGame.Services;
using QuizGame.ViewModels;

namespace QuizGame.Controllers
{
    public class GameController : Controller
    {
        private readonly LeaderboardService _leaderboardService;

        public GameController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        public IActionResult Index(string roomCode)
        {
            return View();
        }

        public async Task<IActionResult> GameSummary(string roomCode)
        {
            try
            {
                var results = await _leaderboardService.GetGameResultsAsync(roomCode);
                return View(results);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
