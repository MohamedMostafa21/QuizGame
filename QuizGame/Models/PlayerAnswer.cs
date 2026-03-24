namespace QuizGame.Models;

public class PlayerAnswer
{
    public int Id { get; set; }
    public int GameQuestionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AnswerOptionId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsWinningAnswer { get; set; } = false;

    public GameQuestion GameQuestion { get; set; } = null!;
    public User User { get; set; } = null!;
    public AnswerOption AnswerOption { get; set; } = null!;
}