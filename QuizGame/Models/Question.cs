namespace QuizGame.Models;

public class Question
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; } = 100;
    public int TimeLimitSeconds { get; set; } = 20;

    public Category Category { get; set; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}