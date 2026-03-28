using Microsoft.AspNetCore.Mvc;

namespace QuizGame.Controllers
{
    public class GameController : Controller
    {
<<<<<<< Updated upstream
        public IActionResult Index(string roomCode)
        {
            return View();
=======
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
        public IActionResult Index(int id)
        {
            GameQuestion? question = _gameQuestionRepo.GetNextPending(id);

            if (question is null)
            {
                return RedirectToAction("GameSummary");
            }

            _gameQuestionRepo.Activate(question.Id);

            GameViewModel vm = new()
            {
                Question = question,
                Player = _gamePlayerRepo
                .GetByGameAndUser(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                Options = _answerOptionRepo.GetByQuestionId(question.Id)
            };

            return View("Index", vm);
>>>>>>> Stashed changes
        }

    }
}
