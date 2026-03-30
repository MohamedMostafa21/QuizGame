namespace QuizGame.ViewModels
{

    public class QuestionResultViewModel
    {
        public int Order { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? WinnerName { get; set; }
        public int PointsAwarded { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
