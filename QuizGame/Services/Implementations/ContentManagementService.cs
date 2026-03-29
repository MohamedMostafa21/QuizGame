using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services.Interfaces;

namespace QuizGame.Services.Implementations;

public class ContentManagementService : IContentManagementService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IGameQuestionRepository _gameQuestionRepository;

    public ContentManagementService(
        ICategoryRepository categoryRepository,
        IQuestionRepository questionRepository,
        IAnswerOptionRepository answerOptionRepository,
        IGameRepository gameRepository,
        IGameQuestionRepository gameQuestionRepository)
    {
        _categoryRepository = categoryRepository;
        _questionRepository = questionRepository;
        _answerOptionRepository = answerOptionRepository;
        _gameRepository = gameRepository;
        _gameQuestionRepository = gameQuestionRepository;
    }

    public List<Category> GetCategories()
    {
        return _categoryRepository
            .GetAll()
            .OrderBy(c => c.Name)
            .ToList();
    }

    public CreateCategoryResult CreateCategory(string name, string description)
    {
        var nameValidationError = ValidateCategoryName(name, null);
        if (!string.IsNullOrWhiteSpace(nameValidationError))
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Message = nameValidationError
            };
        }

        var descriptionValidationError = ValidateCategoryDescription(description);
        if (!string.IsNullOrWhiteSpace(descriptionValidationError))
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Message = descriptionValidationError
            };
        }

        var normalizedName = (name ?? string.Empty).Trim();
        var normalizedDescription = (description ?? string.Empty).Trim();

        var category = new Category
        {
            Name = normalizedName,
            Description = normalizedDescription
        };

        _categoryRepository.Add(category);
        _categoryRepository.Save();

        return new CreateCategoryResult
        {
            Succeeded = true,
            Message = "Category created successfully.",
            CategoryId = category.Id
        };
    }

    public ContentManagementActionResult UpdateCategory(int categoryId, string name, string description)
    {
        var category = _categoryRepository.Get(categoryId);
        if (category == null)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Category not found."
            };
        }

        var nameValidationError = ValidateCategoryName(name, categoryId);
        if (!string.IsNullOrWhiteSpace(nameValidationError))
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = nameValidationError
            };
        }

        var descriptionValidationError = ValidateCategoryDescription(description);
        if (!string.IsNullOrWhiteSpace(descriptionValidationError))
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = descriptionValidationError
            };
        }

        category.Name = (name ?? string.Empty).Trim();
        category.Description = (description ?? string.Empty).Trim();
        _categoryRepository.Update(category);
        _categoryRepository.Save();

        return new ContentManagementActionResult
        {
            Succeeded = true,
            Message = "Category updated successfully."
        };
    }

    public ContentManagementActionResult DeleteCategory(int categoryId)
    {
        var category = _categoryRepository.Get(categoryId);
        if (category == null)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Category not found."
            };
        }

        var hasQuestions = _questionRepository
            .GetAll()
            .Any(q => q.CategoryId == categoryId);

        if (hasQuestions)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Cannot delete this category because it still has questions. Remove those questions first."
            };
        }

        var isUsedByGame = _gameRepository
            .GetAll()
            .Any(g => g.CategoryId == categoryId);

        if (isUsedByGame)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Cannot delete this category because it is already used by existing games."
            };
        }

        _categoryRepository.Delete(categoryId);
        _categoryRepository.Save();

        return new ContentManagementActionResult
        {
            Succeeded = true,
            Message = "Category deleted successfully."
        };
    }

    public CreateQuestionResult CreateQuestion(CreateQuestionRequest request)
    {
        var options = NormalizeOptions(request.Options);
        var questionValidationError = ValidateQuestionInput(request.CategoryId, request.Text, options, request.CorrectOption);
        if (!string.IsNullOrWhiteSpace(questionValidationError))
        {
            return new CreateQuestionResult
            {
                Succeeded = false,
                Message = questionValidationError
            };
        }

        var questionText = (request.Text ?? string.Empty).Trim();

        var question = new Question
        {
            CategoryId = request.CategoryId,
            Text = questionText,
            Points = request.Points,
            TimeLimitSeconds = request.TimeLimitSeconds
        };

        _questionRepository.Add(question);
        _questionRepository.Save();

        for (var i = 0; i < options.Count; i++)
        {
            _answerOptionRepository.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = options[i],
                IsCorrect = request.CorrectOption == i + 1
            });
        }

        _answerOptionRepository.Save();

        return new CreateQuestionResult
        {
            Succeeded = true,
            Message = "Question created successfully.",
            QuestionId = question.Id
        };
    }

    public QuestionEditDetailsResult GetQuestionForEdit(int questionId)
    {
        var question = _questionRepository.Get(questionId);
        if (question == null)
        {
            return new QuestionEditDetailsResult
            {
                Succeeded = false,
                Message = "Question not found."
            };
        }

        var options = _answerOptionRepository
            .GetByQuestionId(questionId)
            .OrderBy(o => o.Id)
            .ToList();

        if (options.Count != 4 || options.All(o => !o.IsCorrect))
        {
            return new QuestionEditDetailsResult
            {
                Succeeded = false,
                Message = "Question options are not valid for editing."
            };
        }

        return new QuestionEditDetailsResult
        {
            Succeeded = true,
            QuestionId = question.Id,
            CategoryId = question.CategoryId,
            Text = question.Text,
            Points = question.Points,
            TimeLimitSeconds = question.TimeLimitSeconds,
            Options = options.Select(o => o.Text).ToList(),
            CorrectOption = options.FindIndex(o => o.IsCorrect) + 1
        };
    }

    public ContentManagementActionResult UpdateQuestion(UpdateQuestionRequest request)
    {
        var question = _questionRepository.Get(request.QuestionId);
        if (question == null)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Question not found."
            };
        }

        if (IsQuestionUsedInGame(request.QuestionId))
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Cannot edit this question because it has already been used in a game."
            };
        }

        var options = NormalizeOptions(request.Options);
        var questionValidationError = ValidateQuestionInput(request.CategoryId, request.Text, options, request.CorrectOption);
        if (!string.IsNullOrWhiteSpace(questionValidationError))
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = questionValidationError
            };
        }

        question.CategoryId = request.CategoryId;
        question.Text = (request.Text ?? string.Empty).Trim();
        question.Points = request.Points;
        question.TimeLimitSeconds = request.TimeLimitSeconds;
        _questionRepository.Update(question);
        _questionRepository.Save();

        var oldOptions = _answerOptionRepository.GetByQuestionId(request.QuestionId);
        foreach (var oldOption in oldOptions)
        {
            _answerOptionRepository.Delete(oldOption.Id);
        }
        _answerOptionRepository.Save();

        for (var i = 0; i < options.Count; i++)
        {
            _answerOptionRepository.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = options[i],
                IsCorrect = request.CorrectOption == i + 1
            });
        }

        _answerOptionRepository.Save();

        return new ContentManagementActionResult
        {
            Succeeded = true,
            Message = "Question updated successfully."
        };
    }

    public ContentManagementActionResult DeleteQuestion(int questionId)
    {
        var question = _questionRepository.Get(questionId);
        if (question == null)
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Question not found."
            };
        }

        if (IsQuestionUsedInGame(questionId))
        {
            return new ContentManagementActionResult
            {
                Succeeded = false,
                Message = "Cannot delete this question because it has already been used in a game."
            };
        }

        var oldOptions = _answerOptionRepository.GetByQuestionId(questionId);
        foreach (var oldOption in oldOptions)
        {
            _answerOptionRepository.Delete(oldOption.Id);
        }
        _answerOptionRepository.Save();

        _questionRepository.Delete(questionId);
        _questionRepository.Save();

        return new ContentManagementActionResult
        {
            Succeeded = true,
            Message = "Question deleted successfully."
        };
    }

    public List<CategoryQuestionCountResult> GetCategoryQuestionCounts()
    {
        var questionCounts = _questionRepository
            .GetAll()
            .GroupBy(q => q.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                QuestionCount = g.Count()
            });

        return _categoryRepository
            .GetAll()
            .GroupJoin(
                questionCounts,
                category => category.Id,
                questionCount => questionCount.CategoryId,
                (category, matches) => new CategoryQuestionCountResult
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Description = category.Description ?? string.Empty,
                    QuestionCount = matches.Select(m => m.QuestionCount).FirstOrDefault()
                })
            .OrderBy(x => x.CategoryName)
            .ToList();
    }

    public List<CategoryQuestionListItemResult> GetQuestionsForCategory(int categoryId)
    {
        var usedQuestionIds = _gameQuestionRepository
            .GetAll()
            .Select(gq => gq.QuestionId)
            .Distinct();

        return _questionRepository
            .GetAll()
            .Where(q => q.CategoryId == categoryId)
            .OrderBy(q => q.Id)
            .Select(q => new CategoryQuestionListItemResult
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                Points = q.Points,
                TimeLimitSeconds = q.TimeLimitSeconds,
                IsInUse = usedQuestionIds.Contains(q.Id)
            })
            .ToList();
    }

    private string? ValidateCategoryName(string name, int? excludeCategoryId)
    {
        var normalizedName = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return "Category name is required.";
        }

        var exists = _categoryRepository
            .GetAll()
            .Any(c => c.Id != excludeCategoryId && c.Name.ToLower() == normalizedName.ToLower());

        if (exists)
        {
            return "This category already exists.";
        }

        return null;
    }

    private string? ValidateQuestionInput(int categoryId, string text, List<string> normalizedOptions, int correctOption)
    {
        var categoryExists = _categoryRepository.Get(categoryId) != null;

        if (!categoryExists)
        {
            return "Please choose a valid category.";
        }

        var questionText = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(questionText))
        {
            return "Question text is required.";
        }

        if (normalizedOptions.Count != 4 || normalizedOptions.Any(string.IsNullOrWhiteSpace))
        {
            return "Please provide exactly 4 non-empty answer options.";
        }

        if (normalizedOptions.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4)
        {
            return "Answer options must be unique.";
        }

        if (correctOption < 1 || correctOption > 4)
        {
            return "Please select a valid correct option.";
        }

        return null;
    }

    private static string? ValidateCategoryDescription(string description)
    {
        var normalizedDescription = (description ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            return "Category description is required.";
        }

        if (normalizedDescription.Length < 6 || normalizedDescription.Length > 220)
        {
            return "Category description must be between 6 and 220 characters.";
        }

        return null;
    }

    private static List<string> NormalizeOptions(List<string> options)
    {
        return (options ?? new List<string>())
            .Select(o => (o ?? string.Empty).Trim())
            .ToList();
    }

    private bool IsQuestionUsedInGame(int questionId)
    {
        return _gameQuestionRepository
            .GetAll()
            .Any(gq => gq.QuestionId == questionId);
    }
}
