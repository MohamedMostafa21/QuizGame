using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.ViewModels;

namespace QuizGame.Services
{
    public class GameService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IGameQuestionRepository _gameQuestionRepository;
        private readonly IGamePlayerRepository _gamePlayerRepository;

        public GameService(
            ICategoryRepository categoryRepository,
            IGameRepository gameRepository,
            IQuestionRepository questionRepository,
            IGameQuestionRepository gameQuestionRepository,
            IGamePlayerRepository gamePlayerRepository)
        {
            _categoryRepository = categoryRepository;
            _gameRepository = gameRepository;
            _questionRepository = questionRepository;
            _gameQuestionRepository = gameQuestionRepository;
            _gamePlayerRepository = gamePlayerRepository;
        }

        public CreateGameViewModel GetCreateGameViewModel()
        {
            return new CreateGameViewModel
            {
                Categories = _categoryRepository.GetAll().ToList()
            };
        }

        public string CreateGame(CreateGameViewModel vm, string hostId)
        {
            string roomCode = GenerateUniqueRoomCode();
            Game game = CreateAndSaveGame(vm, hostId, roomCode);
            CreateAndSaveGameQuestions(vm, game);
            AddHostAsPlayer(game, hostId);
            return roomCode;
        }


        private string GenerateUniqueRoomCode()
        {
            string roomCode;
            do
            {
                roomCode = GenerateRoomCode();
            }
            while (_gameRepository.RoomCodeExists(roomCode));

            return roomCode;
        }

        private Game CreateAndSaveGame(CreateGameViewModel vm, string hostId, string roomCode)
        {
            Game game = new Game
            {
                RoomCode = roomCode,
                HostId = hostId,
                CategoryId = vm.CategoryId,
                QuestionCount = vm.QuestionCount,
                Status = GameStatus.Waiting
            };

            _gameRepository.Add(game);
            return game;
        }

        private void CreateAndSaveGameQuestions(CreateGameViewModel vm, Game game)
        {
            List<Question> questions = _questionRepository
                .GetRandomSample(vm.CategoryId, vm.QuestionCount)
                .ToList();

            List<GameQuestion> gameQuestions = MapToGameQuestions(questions, game.Id);

            _gameQuestionRepository.CreateBulk(gameQuestions);
        }

        private List<GameQuestion> MapToGameQuestions(List<Question> questions, int gameId)
        {
            return questions.Select((question, index) => new GameQuestion
            {
                GameId = gameId,
                QuestionId = question.Id,
                Order = index + 1,
                Status = QuestionStatus.Pending,
                WinnerId = null,
                PointsAwarded = 0
            }).ToList();
        }

        private void AddHostAsPlayer(Game game, string hostId)
        {
            GamePlayer hostPlayer = new GamePlayer
            {
                GameId = game.Id,
                UserId = hostId,
                Score = 0
            };

            _gamePlayerRepository.Add(hostPlayer);
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 5)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}