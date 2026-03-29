using QuizGame.Models;

namespace QuizGame.Services.Interfaces;

public class CreateCategoryResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
}

public class CreateQuestionRequest
{
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimitSeconds { get; set; }
    public List<string> Options { get; set; } = new();
    public int CorrectOption { get; set; }
}

public class CreateQuestionResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? QuestionId { get; set; }
}

public class ContentManagementActionResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UpdateQuestionRequest
{
    public int QuestionId { get; set; }
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimitSeconds { get; set; }
    public List<string> Options { get; set; } = new();
    public int CorrectOption { get; set; }
}

public class QuestionEditDetailsResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int QuestionId { get; set; }
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimitSeconds { get; set; }
    public List<string> Options { get; set; } = new();
    public int CorrectOption { get; set; }
}

public class CategoryQuestionCountResult
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class CategoryQuestionListItemResult
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeLimitSeconds { get; set; }
    public bool IsInUse { get; set; }
}

public interface IContentManagementService
{
    List<Category> GetCategories();
    CreateCategoryResult CreateCategory(string name);
    ContentManagementActionResult UpdateCategory(int categoryId, string name);
    ContentManagementActionResult DeleteCategory(int categoryId);
    CreateQuestionResult CreateQuestion(CreateQuestionRequest request);
    QuestionEditDetailsResult GetQuestionForEdit(int questionId);
    ContentManagementActionResult UpdateQuestion(UpdateQuestionRequest request);
    ContentManagementActionResult DeleteQuestion(int questionId);
    List<CategoryQuestionCountResult> GetCategoryQuestionCounts();
    List<CategoryQuestionListItemResult> GetQuestionsForCategory(int categoryId);
}
