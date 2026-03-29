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
        private readonly GameService _gameService;
        private readonly UserManager<User> _userManager;

        public LobbyController(GameService gameService, UserManager<User> userManager)
        {
            _gameService = gameService;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
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

        [HttpGet]
        public IActionResult Room(string roomCode)
        {
            return View(_gameService.GetRoomViewModel(roomCode, _userManager.GetUserId(User)));
        }

        [HttpGet]
        public IActionResult Join()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Join(JoinRoomViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            JoinGameResult result = _gameService.JoinGame(vm.RoomCode, _userManager.GetUserId(User));

            if (result == JoinGameResult.GameNotFound)
            {
                ModelState.AddModelError(string.Empty, "Game not found. Please check the room code and try again.");
                return View(vm);
            }
            else if (result == JoinGameResult.GameInProgress)
            {
                ModelState.AddModelError(string.Empty, "Unable to join the game. The game is already in progress.");
                return View(vm);
            }
            else
            {
                return RedirectToAction("Room", new { roomCode = vm.RoomCode });
            }


        }

        public IActionResult Leave(string roomCode)
        {
            string userId = _userManager.GetUserId(User);
            _gameService.LeaveGame(roomCode, userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Start(JoinRoomViewModel vm)
        {
           try
            {
                string hostId = _userManager.GetUserId(User);

                _gameService.StartGame(vm, hostId);
               
                int gameId = _gameService.GetGameIdByRoomCode(vm.RoomCode);
                if(gameId == -1)
                {
                    TempData["Error"] = "Game not found. Please check the room code and try again.";
                    return RedirectToAction("Room", new { roomCode = vm.RoomCode });
                };

                return RedirectToAction("Play", "Game", new { roomCode = vm.RoomCode });
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Room", new { roomCode = vm.RoomCode });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Room", new { roomCode = vm.RoomCode });
            }
        }
    }
}
