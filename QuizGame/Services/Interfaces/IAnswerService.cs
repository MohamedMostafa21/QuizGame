namespace QuizGame.Services.Interfaces;

public class AnswerResult
{
    public bool IsCorrect { get; set; }
    public bool IsWinningAnswer { get; set; }  
    public int PointsAwarded { get; set; }
    public int CorrectAnswerOptionId { get; set; }
    public string Message { get; set; } = string.Empty; 
    public bool IsRejected { get; set; }  
}

public interface IAnswerService
{
    Task<AnswerResult> SubmitAsync(int gameQuestionId, string userId, int answerOptionId);
}