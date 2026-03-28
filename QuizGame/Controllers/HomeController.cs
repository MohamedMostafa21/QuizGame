
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using QuizGame.Services;

using QuizGame.Models;

using System.Threading.Tasks;

namespace QuizGame.Controllers

{

    public class HomeController : Controller

    {

        private readonly LeaderboardService _leaderboardService;



        public HomeController(LeaderboardService leaderboardService)

        {

            _leaderboardService = leaderboardService;

        }


        /// <summary>
        /// Displays the home page with global leaderboard of top players
        /// </summary>
        /// <param name="count">Number of top players to display (default 10)</param>
        /// <returns>The home page view with leaderboard data</returns>
        public async Task<IActionResult> Index(int count = 10)
        {
            try
            {
                var topPlayers = await _leaderboardService.GetGlobalTopAsync(count);
                return View(topPlayers);
            }
            catch (Exception)
            {
                // Log error in production
                return View(new List<User>());
            }
        }
    }
}
