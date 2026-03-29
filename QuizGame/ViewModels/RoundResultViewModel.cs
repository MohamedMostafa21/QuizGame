namespace QuizGame.ViewModels
{
    public class RoundResultViewModel
    {
        public string? WinnerUserName { get; set; }   
        public int PointsAwarded { get; set; }
        public int CorrectAnswerOptionId { get; set; } 
        public List<PlayerScoreViewModel> Scoreboard { get; set; } = new();
    }
}
