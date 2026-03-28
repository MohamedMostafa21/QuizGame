namespace QuizGame.ViewModels
{
    /// <summary>
    /// View model for a player's ranking in a game
    /// </summary>
    public class PlayerRankingViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Rank { get; set; }
    }
}
