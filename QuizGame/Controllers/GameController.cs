using Microsoft.AspNetCore.Mvc;

namespace QuizGame.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index(string roomCode)
        {
            return View();
        }

    }
}
