using QuizGame.Models;

namespace QuizGame.ViewModels
{
    public class RoomViewModel
    {
        public string HostName { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string CategoryName { get; set; }
        public int QuestionCount { get; set; }
        public ICollection<GamePlayer>? GamePlayers { get; set; }


    }
}
