namespace QuizGame.ViewModels;

public class LobbyIndexViewModel
{
    public List<PendingGameViewModel> PendingGames { get; set; } = new();
}

public class PendingGameViewModel
{
    public int GameId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = "All Categories";
    public int QuestionCount { get; set; }
    public int PlayerCount { get; set; }
    public bool IsUserJoined { get; set; }
}
