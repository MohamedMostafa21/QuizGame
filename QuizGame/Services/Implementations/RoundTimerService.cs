using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using QuizGame.Hubs;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services;
using QuizGame.ViewModels;

namespace QuizGame.Services.Implementations;

public class RoundTimerService
{
    // One CancellationTokenSource per active GameQuestion
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _timers = new();
    private readonly ConcurrentDictionary<int, byte> _finalizedGames = new();

    private readonly IHubContext<GameHub> _hub;
    private readonly IServiceScopeFactory _scopeFactory;

    public RoundTimerService(IHubContext<GameHub> hub, IServiceScopeFactory scopeFactory)
    {
        _hub = hub;
        _scopeFactory = scopeFactory;
    }

    // Called when a question becomes Active
    public void StartTimer(int gameQuestionId, int gameId, string roomCode, int seconds)
    {
        CancelTimer(gameQuestionId); // safety - cancel any leftover timer

        var cts = new CancellationTokenSource();
        _timers[gameQuestionId] = cts;

        // Fire and forget - runs in background
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(seconds * 1000, cts.Token);
                // If we reach here, nobody won in time
                await OnTimerExpiredAsync(gameQuestionId, gameId, roomCode);
            }
            catch (OperationCanceledException)
            {
                // A winner was found and cancelled this timer - do nothing
            }
        });
    }

    // Called by the Hub immediately after a winning answer is confirmed
    public void CancelTimer(int gameQuestionId)
    {
        if (_timers.TryRemove(gameQuestionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    // Fires when time runs out with no winner
    private async Task OnTimerExpiredAsync(int gameQuestionId, int gameId, string roomCode)
    {
        using var scope = _scopeFactory.CreateScope();
        var gameQRepo = scope.ServiceProvider.GetRequiredService<IGameQuestionRepository>();

        var gq = await gameQRepo.GetByIdAsync(gameQuestionId);

        // Another request may have closed it already - double check
        if (gq == null || gq.Status != QuestionStatus.Active) return;

        await gameQRepo.CloseAsync(gameQuestionId, null, 0);

        var correctId = gq.Question.AnswerOptions.First(o => o.IsCorrect).Id;
        var scoreboard = await BuildScoreboardAsync(scope, gameId);

        await _hub.Clients.Group(roomCode).SendAsync("RoundTimedOut", new RoundResultViewModel
        {
            WinnerUserName = null,
            PointsAwarded = 0,
            CorrectAnswerOptionId = correctId,
            Scoreboard = scoreboard
        });

        await AdvanceGameAsync(gameId, roomCode, scope);
    }

    // Advance to the next question or end the game.
    // Called by both the Hub (after a winner) and by OnTimerExpiredAsync (after timeout)
    public async Task AdvanceGameAsync(int gameId, string roomCode, IServiceScope? existingScope = null)
    {
        // Use provided scope or create a new one
        var scope = existingScope ?? _scopeFactory.CreateScope();
        bool shouldDispose = existingScope == null;

        try
        {
            var gameQRepo = scope.ServiceProvider.GetRequiredService<IGameQuestionRepository>();
            var gameRepo = scope.ServiceProvider.GetRequiredService<IGameRepository>();

            var next = await gameQRepo.GetNextPendingAsync(gameId);

            if (next != null)
            {
                // There is a next question - activate it and broadcast
                await gameQRepo.ActivateAsync(next.Id);

                var vm = new ActiveQuestionViewModel
                {
                    GameQuestionId = next.Id,
                    Order = next.Order,
                    TotalQuestions = (await gameRepo.GetByIdAsync(gameId))!.QuestionCount,
                    Text = next.Question.Text,
                    Points = next.Question.Points,
                    TimeLimitSeconds = next.Question.TimeLimitSeconds,
                    Options = next.Question.AnswerOptions.Select(o => new AnswerOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text
                        // no IsCorrect
                    }).ToList()
                };

                await _hub.Clients.Group(roomCode).SendAsync("NextQuestion", vm);

                StartTimer(next.Id, gameId, roomCode, next.Question.TimeLimitSeconds);
            }
            else
            {
                // No more questions - game over
                if (!_finalizedGames.TryAdd(gameId, 0))
                    return;

                var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardService>();

                await gameRepo.UpdateStatusAsync(gameId, GameStatus.Finished);
                await leaderboardService.FinalizeGameAsync(gameId);

                var scoreboard = await BuildScoreboardAsync(scope, gameId);
                await _hub.Clients.Group(roomCode).SendAsync("GameOver", scoreboard);
            }
        }
        finally
        {
            if (shouldDispose) scope.Dispose();
        }
    }

    private async Task<List<PlayerScoreViewModel>> BuildScoreboardAsync(IServiceScope scope, int gameId)
    {
        var gpRepo = scope.ServiceProvider.GetRequiredService<IGamePlayerRepository>();
        var players = await gpRepo.GetByGameAsync(gameId);
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