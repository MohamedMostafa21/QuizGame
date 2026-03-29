using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using QuizGame.Hubs;
using QuizGame.Models;
using QuizGame.Repositories.Implementations;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services.Implementations;
using QuizGame.ViewModels;
using System;
using System.Linq;

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
        private readonly IHubContext<GameHub> _gameHubContext;
        private readonly RoundTimerService _roundTimerService;

        public GameService(
            ICategoryRepository categoryRepository,
            IGameRepository gameRepository,
            IQuestionRepository questionRepository,
            IGameQuestionRepository gameQuestionRepository,
            IGamePlayerRepository gamePlayerRepository,
            UserManager<User> userManager,
            IHubContext<GameHub> gameHubContext,
            RoundTimerService roundTimerService)

        {
            _categoryRepository = categoryRepository;
            _gameRepository = gameRepository;
            _questionRepository = questionRepository;
            _gameQuestionRepository = gameQuestionRepository;
            _gamePlayerRepository = gamePlayerRepository;
            _userManager = userManager;
            _gameHubContext = gameHubContext;
            _roundTimerService = roundTimerService;
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

        public RoomViewModel GetRoomViewModel(string roomCode, string userId)
        {
            Game? game = _gameRepository.GetByRoomCode(roomCode);

            if (game == null)
                return null!;

            User? hostUser = _userManager.FindByIdAsync(game.HostId).GetAwaiter().GetResult();

            Category? category = null;
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
                HostId = game.HostId,
                UserId = userId,
                HostName = hostUser?.UserName,
                RoomCode = game.RoomCode,
                CategoryName = category?.Name ?? "All Categories",
                QuestionCount = game.QuestionCount,
                GamePlayers = players,
            };
        }

        public JoinGameResult JoinGame(string roomCode, string userId)
        {
            if(!_gameRepository.RoomCodeExists(roomCode))
                return JoinGameResult.GameNotFound;

            Game? game = _gameRepository.GetByRoomCode(roomCode);

            if(game.Status != GameStatus.Waiting)
                return JoinGameResult.GameInProgress;
            
            if(_gamePlayerRepository.GetByGame(game.Id).Any(p => p.UserId == userId))
                return JoinGameResult.Success;

            GamePlayer newPlayer = new GamePlayer
            {
                GameId = game.Id,
                UserId = userId,
                Score = 0
            };
            _gamePlayerRepository.Add(newPlayer);
            _gamePlayerRepository.Save();
            _gameHubContext.Clients.Group(game.RoomCode).SendAsync("PlayerJoined", _userManager.FindByIdAsync(userId).GetAwaiter().GetResult()?.UserName);
            return JoinGameResult.Success;
        }

        public void StartGame(JoinRoomViewModel vm, string requestingUserId)
        {
            Game? game = _gameRepository.GetByRoomCode(vm.RoomCode);

            if (game == null)
                throw new Exception("Game not found.");

            if (game.HostId != requestingUserId)
                throw new UnauthorizedAccessException("Only the host can start the game.");

            if (game.Status != GameStatus.Waiting)
                throw new Exception("Game has already started or finished.");

            game.Status = GameStatus.InProgress;
            _gameRepository.Update(game);
            _gameRepository.Save();

            GameQuestion? firstQuestion = _gameQuestionRepository.GetNextPending(game.Id);
            if (firstQuestion == null)
                throw new Exception("No questions available for this game.");

            _gameQuestionRepository.Activate(firstQuestion.Id);
            _gameQuestionRepository.Save();

            _gameHubContext.Clients.Group(vm.RoomCode).SendAsync("NavigateToGame", vm.RoomCode);
            _roundTimerService.StartTimer(firstQuestion.Id, game.Id, vm.RoomCode, firstQuestion.Question.TimeLimitSeconds);
        }

        internal void LeaveGame(string roomCode, string? userId)
        {
            Game? game = _gameRepository.GetByRoomCode(roomCode);

            int gameId = game?.Id ?? -1;

            _gamePlayerRepository.RemovePlayer(gameId, userId);
            _gamePlayerRepository.Save();

            _gameHubContext.Clients.Group(roomCode)
                .SendAsync("PlayerLeft", _userManager.FindByIdAsync(userId).GetAwaiter().GetResult()?.UserName);
        }

        internal int GetGameIdByRoomCode(string roomCode)
        {
            Game? game = _gameRepository.GetByRoomCode(roomCode);
            return game?.Id ?? -1;
        }
    }
}