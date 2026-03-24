namespace QuizGame.Models;

public class Game
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int QuestionCount { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Waiting;

    public User Host { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    public ICollection<GameQuestion> GameQuestions { get; set; } = new List<GameQuestion>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}