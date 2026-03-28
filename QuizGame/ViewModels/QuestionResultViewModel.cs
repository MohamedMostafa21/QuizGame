namespace QuizGame.ViewModels
{
    /// <summary>
    /// View model for a question's result in a game
    /// </summary>
    public class QuestionResultViewModel
    {
        public int Order { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? WinnerName { get; set; }
        public int PointsAwarded { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
