namespace QuizGame.Models;

public class GameQuestion
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int QuestionId { get; set; }
    public int Order { get; set; }
    public QuestionStatus Status { get; set; } = QuestionStatus.Pending;
    public string? WinnerId { get; set; }
    public int PointsAwarded { get; set; } = 0;

    public Game Game { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public User? Winner { get; set; }
    public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();
}