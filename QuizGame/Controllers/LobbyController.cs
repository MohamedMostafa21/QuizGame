using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizGame.Models;
using QuizGame.Services;
using QuizGame.ViewModels;

namespace QuizGame.Controllers
{
    [Authorize]
    public class LobbyController : Controller
    {
        private GameService _gameService;
        private readonly UserManager<User> _userManager;

        public LobbyController(GameService gameService, UserManager<User> userManager)
        {
            _gameService = gameService;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Create()
        {
            
            return View(_gameService.GetCreateGameViewModel());
        }

        [HttpPost]
        public IActionResult Create(CreateGameViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = _gameService.GetCreateGameViewModel().Categories;
                return View(vm);
            }

            string hostId = _userManager.GetUserId(User);
            string roomCode = _gameService.CreateGame(vm, hostId);

            return RedirectToAction("Room", new { roomCode });
        }

        public IActionResult Room(string roomCode)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Join()
        {
            return View();
        }
        //[HttpPost]
        //public IActionResult Join()
        //{
        //    return View();
        //}
    }
}
