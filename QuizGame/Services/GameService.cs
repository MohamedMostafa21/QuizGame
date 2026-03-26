using System;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace QuizGame.Services
{
    public class GameService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IGameQuestionRepository _gameQuestionRepository;
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly UserManager<User> _userManager;

        public GameService(
            ICategoryRepository categoryRepository,
            IGameRepository gameRepository,
            IQuestionRepository questionRepository,
            IGameQuestionRepository gameQuestionRepository,
            IGamePlayerRepository gamePlayerRepository,
            UserManager<User> userManager)
        {
            _categoryRepository = categoryRepository;
            _gameRepository = gameRepository;
            _questionRepository = questionRepository;
            _gameQuestionRepository = gameQuestionRepository;
            _gamePlayerRepository = gamePlayerRepository;
            _userManager = userManager;
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
            _gameRepository.Save();
            return game;
        }

        private void CreateAndSaveGameQuestions(CreateGameViewModel vm, Game game)
        {
            List<Question> questions = _questionRepository
                .GetRandomSample(vm.CategoryId, vm.QuestionCount)
                .ToList();

            List<GameQuestion> gameQuestions = MapToGameQuestions(questions, game.Id);

            _gameQuestionRepository.CreateBulk(gameQuestions);
            _gameQuestionRepository.Save();
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
            _gamePlayerRepository.Save();
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Range(0, 5)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        public RoomViewModel GetRoomViewModel(string roomCode)
        {
            Game? game = _gameRepository.GetByRoomCode(roomCode);

            if (game == null)
                return null!;

            User? hostUser = _userManager.FindByIdAsync(game.HostId).GetAwaiter().GetResult();

            Category? category = game.Category;
            if (game.CategoryId != null)
            {
                category = _categoryRepository.Get(game.CategoryId.Value);
            }

            List<GamePlayer> players = _gamePlayerRepository.GetByGame(game.Id).ToList();
            foreach (GamePlayer p in players)
            {
                if (p.User == null)
                {
                    p.User = _userManager.FindByIdAsync(p.UserId).GetAwaiter().GetResult()!;
                }
            }

            return new RoomViewModel
            {
                HostName = hostUser?.UserName,
                RoomCode = game.RoomCode,
                CategoryName = category?.Name ?? "All Categories",
                QuestionCount = game.QuestionCount,
                GamePlayers = players,
            };
        }
    }
}