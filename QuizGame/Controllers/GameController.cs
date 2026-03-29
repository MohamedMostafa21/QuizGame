using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services;
using QuizGame.ViewModels;

namespace QuizGame.Controllers;

[Authorize]
public class GameController : Controller
{
    private readonly IGameRepository _games;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly IGameQuestionRepository _gameQuestions;
    private readonly LeaderboardService _leaderboardService;
    private readonly UserManager<User> _userManager;

    public GameController(
        IGameRepository games,
        IGamePlayerRepository gamePlayers,
        IGameQuestionRepository gameQuestions,
        LeaderboardService leaderboardService,
        UserManager<User> userManager)
    {
        _games = games;
        _gamePlayers = gamePlayers;
        _gameQuestions = gameQuestions;
        _leaderboardService = leaderboardService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Play(string roomCode)
    {
        var userId = _userManager.GetUserId(User)!;

        var game = await _games.GetByRoomCodeAsync(roomCode);

        if (game == null)
            return NotFound();

        if (game.Status == GameStatus.Waiting)
            return RedirectToAction("Room", "Lobby", new { roomCode });

        if (game.Status == GameStatus.Finished)
            return RedirectToAction("Summary", new { roomCode });

        if (!await _gamePlayers.IsUserInGameAsync(game.Id, userId))
            return RedirectToAction("Join", "Lobby");

        var activeQuestion = await _gameQuestions.GetActiveAsync(game.Id);

        var vm = new GamePlayViewModel
        {
            RoomCode = roomCode,
            GameId = game.Id,
            ActiveQuestion = activeQuestion == null ? null : new ActiveQuestionViewModel
            {
                GameQuestionId = activeQuestion.Id,
                Order = activeQuestion.Order,
                TotalQuestions = game.QuestionCount,
                Text = activeQuestion.Question.Text,
                Points = activeQuestion.Question.Points,
                TimeLimitSeconds = activeQuestion.Question.TimeLimitSeconds,
                Options = activeQuestion.Question.AnswerOptions
                    .Select(o => new AnswerOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text
                    }).ToList()
            }
        };

        return View(vm);
    }

    public async Task<IActionResult> Summary(string roomCode)
    {
        var game = await _games.GetByRoomCodeAsync(roomCode);
        if (game == null) return NotFound();

        if (game.Status != GameStatus.Finished)
            return RedirectToAction(nameof(Play), new { roomCode });

        var vm = await _leaderboardService.GetGameResultsAsync(roomCode);
        return View(vm);
    }
}