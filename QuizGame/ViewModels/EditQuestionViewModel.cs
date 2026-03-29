using QuizGame.Models;
using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels;

public class EditQuestionViewModel
{
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Question text must be between 5 and 300 characters.")]
    [Display(Name = "Question")]
    public string Text { get; set; } = string.Empty;

    [Range(50, 2000, ErrorMessage = "Points must be between 50 and 2000.")]
    public int Points { get; set; } = 100;

    [Range(5, 120, ErrorMessage = "Time limit must be between 5 and 120 seconds.")]
    [Display(Name = "Time Limit (seconds)")]
    public int TimeLimitSeconds { get; set; } = 20;

    [Required(ErrorMessage = "Option A is required.")]
    [StringLength(120)]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [StringLength(120)]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [StringLength(120)]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [StringLength(120)]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Range(1, 4, ErrorMessage = "Correct option must be between 1 and 4.")]
    [Display(Name = "Correct Option")]
    public int CorrectOption { get; set; } = 1;

    public List<Category> Categories { get; set; } = new();
}
