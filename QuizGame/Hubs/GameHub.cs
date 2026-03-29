using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services.Implementations;
using QuizGame.Services.Interfaces;
using QuizGame.ViewModels;

namespace QuizGame.Hubs;

[Authorize] 
public class GameHub : Hub
{
    private readonly IGameRepository _games;
    private readonly IGameQuestionRepository _gameQuestions;
    private readonly IGamePlayerRepository _gamePlayers;
    private readonly IAnswerService _answerService;
    private readonly RoundTimerService _timerService;
    private readonly UserManager<User> _userManager;

    public GameHub(
        IGameRepository games,
        IGameQuestionRepository gameQuestions,
        IGamePlayerRepository gamePlayers,
        IAnswerService answerService,
        RoundTimerService timerService,
        UserManager<User> userManager)
    {
        _games = games;
        _gameQuestions = gameQuestions;
        _gamePlayers = gamePlayers;
        _answerService = answerService;
        _timerService = timerService;
        _userManager = userManager;
    }

    public async Task JoinGameRoom(string roomCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        var user = await _userManager.GetUserAsync(Context.User!);
        await Clients.Group(roomCode).SendAsync("PlayerConnected", user?.UserName ?? "Someone");
    }

    public async Task SubmitAnswer(string roomCode, int gameQuestionId, int answerOptionId)
    {
        var userId = _userManager.GetUserId(Context.User!)!;

        var result = await _answerService.SubmitAsync(gameQuestionId, userId, answerOptionId);

        if (result.IsRejected)
        {
            await Clients.Caller.SendAsync("AnswerRejected", result.Message);
            return;
        }

        if (result.IsWinningAnswer)
        {
            _timerService.CancelTimer(gameQuestionId);

            var game = await _games.GetByRoomCodeAsync(roomCode);
            var scoreboard = await BuildScoreboardAsync(game!.Id);
            var winnerName = (await _userManager.FindByIdAsync(userId))?.UserName ?? "Unknown";

            await Clients.Group(roomCode).SendAsync("RoundWon", new RoundResultViewModel
            {
                WinnerUserName = winnerName,
                PointsAwarded = result.PointsAwarded,
                CorrectAnswerOptionId = result.CorrectAnswerOptionId,
                Scoreboard = scoreboard
            });

            await Task.Delay(3000);
            await _timerService.AdvanceGameAsync(game!.Id, roomCode);
        }
        else
        {
            await Clients.Caller.SendAsync("AnswerResult", new
            {
                isCorrect = result.IsCorrect,
                message = result.IsCorrect ? "Correct! But someone was faster." : "Wrong answer."
            });
        }
    }

    public async Task SendChatMessage(string roomCode, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var user = await _userManager.GetUserAsync(Context.User!);

        await Clients.Group(roomCode).SendAsync("ChatMessageReceived", new ChatMessageViewModel
        {
            UserName = user?.UserName ?? "Unknown",
            Text = text.Trim(),
            SentAt = DateTime.UtcNow.ToString("HH:mm")
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<List<PlayerScoreViewModel>> BuildScoreboardAsync(int gameId)
    {
        var players = await _gamePlayers.GetByGameAsync(gameId);
        return players
            .OrderByDescending(p => p.Score)
            .Select(p => new PlayerScoreViewModel
            {
                UserName = p.User.UserName ?? "Unknown",
                Score = p.Score
            })
            .ToList();
    }
}