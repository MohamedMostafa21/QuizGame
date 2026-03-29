namespace QuizGame.ViewModels
{
    public class GamePlayViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public int GameId { get; set; }
        public ActiveQuestionViewModel? ActiveQuestion { get; set; }
    }
}
