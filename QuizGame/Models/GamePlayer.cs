namespace QuizGame.Models;

public class GamePlayer
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Score { get; set; } = 0;

    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
}