using QuizGame.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels
{
    public class CreateGameViewModel
    {
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Question count is required.")]
        [Range(1, 20, ErrorMessage = "Question count must be between 1 and 20.")]
        [Display(Name = "Number of Questions")]
        public int QuestionCount { get; set; }
        public List<Category> Categories { get; set; }
    }
}
