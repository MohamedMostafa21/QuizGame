namespace QuizGame.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
}