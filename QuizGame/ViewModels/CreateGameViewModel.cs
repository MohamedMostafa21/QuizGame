using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels
{
    public class CreateGameViewModel
    {
        [Required(ErrorMessage = "Please choose a package.")]
        [Display(Name = "Question Package")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Question count is required.")]
        [Range(1, 200, ErrorMessage = "Question count must be at least 1.")]
        [Display(Name = "Number of Questions")]
        public int QuestionCount { get; set; }

        public List<GamePackageOptionViewModel> Packages { get; set; } = new();
    }

    public class GamePackageOptionViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AvailableQuestionCount { get; set; }
    }
}
