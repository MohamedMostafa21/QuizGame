using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels;

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 60 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category description is required.")]
    [StringLength(220, MinimumLength = 6, ErrorMessage = "Description must be between 6 and 220 characters.")]
    [Display(Name = "Category Description")]
    public string Description { get; set; } = string.Empty;

    public int? SelectedCategoryId { get; set; }
    public int? EditingCategoryId { get; set; }
    public string SelectedCategoryName { get; set; } = string.Empty;
    public string SelectedCategoryDescription { get; set; } = string.Empty;
    public List<CategoryQuestionCountViewModel> CategoryQuestionCounts { get; set; } = new();
    public List<CategoryQuestionListItemViewModel> SelectedCategoryQuestions { get; set; } = new();
}

public class CategoryQuestionCountViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class CategoryQuestionListItemViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimitSeconds { get; set; }
    public bool IsInUse { get; set; }
}
