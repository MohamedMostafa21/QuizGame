using Microsoft.AspNetCore.Mvc;
using QuizGame.Models;
using QuizGame.Repositories.Implementations;
using QuizGame.Repositories.Interfaces;
using QuizGame.ViewModels;
using System.Security.Claims;

namespace QuizGame.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameQuestionRepository _gameQuestionRepo;
        private readonly IGamePlayerRepository _gamePlayerRepo;
        private readonly IAnswerOptionRepository _answerOptionRepo;

        public GameController(IGameQuestionRepository gameQuestionRepository, IGamePlayerRepository gamePlayerRepository, IAnswerOptionRepository answerOptionRepository)
        {
            _gameQuestionRepo = gameQuestionRepository;
            _gamePlayerRepo = gamePlayerRepository;
            _answerOptionRepo = answerOptionRepository;
        }

        [HttpGet]
        public IActionResult Index(string roomCode)
        {
            //GameQuestion? question = _gameQuestionRepo.GetNextPending(id);

            //if (question is null)
            //{
            //    return RedirectToAction("GameSummary");
            //}

            //_gameQuestionRepo.Activate(question.Id);

            //GameViewModel vm = new()
            //{
            //    Question = question,
            //    Player = _gamePlayerRepo
            //    .GetByGameAndUser(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
            //    Options = _answerOptionRepo.GetByQuestionId(question.Id)
            //};

            //return View("Index", vm);
            return View();
        }

        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
