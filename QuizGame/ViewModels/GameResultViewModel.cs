namespace QuizGame.ViewModels
{
    /// <summary>
    /// View model for game results summary
    /// </summary>
    public class GameResultViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public List<PlayerRankingViewModel> FinalRankings { get; set; } = new List<PlayerRankingViewModel>();
        public List<QuestionResultViewModel> QuestionResults { get; set; } = new List<QuestionResultViewModel>();
    }
}
