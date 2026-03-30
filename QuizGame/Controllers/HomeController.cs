
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


        public async Task<IActionResult> Index(int count = 10)
        {
            try
            {
                var topPlayers = await _leaderboardService.GetGlobalTopAsync(count);
                return View(topPlayers);
            }
            catch (Exception)
            {
                return View(new List<User>());
            }
        }
    }
}
