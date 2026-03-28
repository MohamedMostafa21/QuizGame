using QuizGame.Models;

namespace QuizGame.ViewModels
{
    public class GameViewModel
    {
        public GameQuestion Question { get; set; }
        public GamePlayer Player { get; set; }
        public List<AnswerOption> Options { get; set; } 
    }
}
