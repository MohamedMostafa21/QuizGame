namespace QuizGame.ViewModels
{
    public class ActiveQuestionViewModel
    {
        public int GameQuestionId { get; set; }
        public int Order { get; set; }          
        public int TotalQuestions { get; set; } 
        public string Text { get; set; } = string.Empty;
        public int Points { get; set; }
        public int TimeLimitSeconds { get; set; }
        public List<AnswerOptionViewModel> Options { get; set; } = new();
    }
}
