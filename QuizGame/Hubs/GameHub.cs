using Microsoft.AspNetCore.SignalR;
using QuizGame.Services;

namespace QuizGame.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;

    public GameHub(GameService gameService)
    {
        _gameService = gameService;
    }

    public async Task JoinRoomGroup(string roomCode, string UserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        var vm = _gameService.GetRoomViewModel(roomCode, UserId);
        var playerNames = vm?.GamePlayers?.Select(p => p.User?.UserName ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new System.Collections.Generic.List<string>();

        await Clients.Caller.SendAsync("InitialPlayers", playerNames);
    }

    public async Task LeaveRoomGroup(string roomCode, string UserId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

        var vm = _gameService.GetRoomViewModel(roomCode, UserId);
        var playerNames = vm?.GamePlayers?.Select(p => p.User?.UserName ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new System.Collections.Generic.List<string>();

        await Clients.Group(roomCode).SendAsync("UpdatePlayers", playerNames);
    }

    public async Task StartGame(string roomCode)
    {
        await Clients.Group(roomCode).SendAsync("NavigateToGame", roomCode);
    }
}