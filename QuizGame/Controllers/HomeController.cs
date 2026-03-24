using Microsoft.AspNetCore.Mvc;

namespace QuizGame.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
