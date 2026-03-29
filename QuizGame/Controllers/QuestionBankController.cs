using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGame.Services.Interfaces;
using QuizGame.ViewModels;

namespace QuizGame.Controllers;

[Authorize]
public class QuestionBankController : Controller
{
    private readonly IContentManagementService _contentManagementService;

    public QuestionBankController(IContentManagementService contentManagementService)
    {
        _contentManagementService = contentManagementService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Categories(int? categoryId, int? editCategoryId)
    {
        return View(BuildCategoriesViewModel(categoryId, editCategoryId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Categories(CreateCategoryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildCategoriesViewModel(vm.SelectedCategoryId, vm.EditingCategoryId, vm));
        }

        if (vm.EditingCategoryId.HasValue)
        {
            var updateResult = _contentManagementService.UpdateCategory(vm.EditingCategoryId.Value, vm.Name, vm.Description);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, updateResult.Message);
                return View(BuildCategoriesViewModel(vm.SelectedCategoryId, vm.EditingCategoryId, vm));
            }

            TempData["ManageSuccess"] = updateResult.Message;
            return RedirectToAction(nameof(Categories), new { categoryId = vm.EditingCategoryId.Value });
        }

        var createResult = _contentManagementService.CreateCategory(vm.Name, vm.Description);
        if (!createResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, createResult.Message);
            return View(BuildCategoriesViewModel(vm.SelectedCategoryId, vm.EditingCategoryId, vm));
        }

        TempData["ManageSuccess"] = createResult.Message;
        return RedirectToAction(nameof(Categories), new { categoryId = createResult.CategoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCategory(int categoryId, int? selectedCategoryId)
    {
        var result = _contentManagementService.DeleteCategory(categoryId);

        if (result.Succeeded)
        {
            TempData["ManageSuccess"] = result.Message;
            if (selectedCategoryId == categoryId)
            {
                selectedCategoryId = null;
            }
        }
        else
        {
            TempData["ManageError"] = result.Message;
        }

        return RedirectToAction(nameof(Categories), new { categoryId = selectedCategoryId });
    }

    [HttpGet]
    public IActionResult Questions()
    {
        var vm = new CreateQuestionViewModel
        {
            Categories = _contentManagementService.GetCategories()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Questions(CreateQuestionViewModel vm)
    {
        vm.Categories = _contentManagementService.GetCategories();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (!vm.CategoryId.HasValue)
        {
            ModelState.AddModelError(nameof(vm.CategoryId), "Please choose a category.");
            return View(vm);
        }

        var request = new CreateQuestionRequest
        {
            CategoryId = vm.CategoryId.Value,
            Text = vm.Text,
            Points = vm.Points,
            TimeLimitSeconds = vm.TimeLimitSeconds,
            CorrectOption = vm.CorrectOption,
            Options = new List<string> { vm.OptionA, vm.OptionB, vm.OptionC, vm.OptionD }
        };

        var result = _contentManagementService.CreateQuestion(request);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(vm);
        }

        TempData["ManageSuccess"] = result.Message;
        return RedirectToAction(nameof(Questions));
    }

    [HttpGet]
    public IActionResult EditQuestion(int questionId, int? categoryId)
    {
        var details = _contentManagementService.GetQuestionForEdit(questionId);
        if (!details.Succeeded)
        {
            TempData["ManageError"] = details.Message;
            return RedirectToAction(nameof(Categories), new { categoryId });
        }

        if (details.Options.Count != 4)
        {
            TempData["ManageError"] = "Question options are not valid for editing.";
            return RedirectToAction(nameof(Categories), new { categoryId });
        }

        var vm = new EditQuestionViewModel
        {
            QuestionId = details.QuestionId,
            CategoryId = details.CategoryId,
            Text = details.Text,
            Points = details.Points,
            TimeLimitSeconds = details.TimeLimitSeconds,
            OptionA = details.Options[0],
            OptionB = details.Options[1],
            OptionC = details.Options[2],
            OptionD = details.Options[3],
            CorrectOption = details.CorrectOption,
            Categories = _contentManagementService.GetCategories()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditQuestion(EditQuestionViewModel vm)
    {
        vm.Categories = _contentManagementService.GetCategories();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (!vm.CategoryId.HasValue)
        {
            ModelState.AddModelError(nameof(vm.CategoryId), "Please choose a category.");
            return View(vm);
        }

        var request = new UpdateQuestionRequest
        {
            QuestionId = vm.QuestionId,
            CategoryId = vm.CategoryId.Value,
            Text = vm.Text,
            Points = vm.Points,
            TimeLimitSeconds = vm.TimeLimitSeconds,
            CorrectOption = vm.CorrectOption,
            Options = new List<string> { vm.OptionA, vm.OptionB, vm.OptionC, vm.OptionD }
        };

        var result = _contentManagementService.UpdateQuestion(request);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(vm);
        }

        TempData["ManageSuccess"] = result.Message;
        return RedirectToAction(nameof(Categories), new { categoryId = vm.CategoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteQuestion(int questionId, int? categoryId)
    {
        var result = _contentManagementService.DeleteQuestion(questionId);

        if (result.Succeeded)
        {
            TempData["ManageSuccess"] = result.Message;
        }
        else
        {
            TempData["ManageError"] = result.Message;
        }

        return RedirectToAction(nameof(Categories), new { categoryId });
    }

    private CreateCategoryViewModel BuildCategoriesViewModel(int? selectedCategoryId, int? editCategoryId = null, CreateCategoryViewModel? source = null)
    {
        var vm = source ?? new CreateCategoryViewModel();
        vm.SelectedCategoryId = selectedCategoryId;
        vm.EditingCategoryId = editCategoryId;

        var categoryQuestionCounts = _contentManagementService.GetCategoryQuestionCounts();
        vm.CategoryQuestionCounts = categoryQuestionCounts
            .Select(x => new CategoryQuestionCountViewModel
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                Description = x.Description,
                QuestionCount = x.QuestionCount
            })
            .ToList();

        if (!selectedCategoryId.HasValue)
        {
            return vm;
        }

        var selectedCategory = vm.CategoryQuestionCounts
            .FirstOrDefault(x => x.CategoryId == selectedCategoryId.Value);

        if (selectedCategory == null)
        {
            vm.SelectedCategoryId = null;
            return vm;
        }

        vm.SelectedCategoryName = selectedCategory.CategoryName;
        vm.SelectedCategoryDescription = selectedCategory.Description;
        vm.SelectedCategoryQuestions = _contentManagementService
            .GetQuestionsForCategory(selectedCategory.CategoryId)
            .Select(x => new CategoryQuestionListItemViewModel
            {
                QuestionId = x.QuestionId,
                QuestionText = x.QuestionText,
                Points = x.Points,
                TimeLimitSeconds = x.TimeLimitSeconds,
                IsInUse = x.IsInUse
            })
            .ToList();

        if (!vm.EditingCategoryId.HasValue)
        {
            return vm;
        }

        var editingCategory = vm.CategoryQuestionCounts
            .FirstOrDefault(x => x.CategoryId == vm.EditingCategoryId.Value);

        if (editingCategory == null)
        {
            vm.EditingCategoryId = null;
            return vm;
        }

        if (source == null || string.IsNullOrWhiteSpace(source.Name))
        {
            vm.Name = editingCategory.CategoryName;
        }

        if (source == null || string.IsNullOrWhiteSpace(source.Description))
        {
            vm.Description = editingCategory.Description;
        }

        return vm;
    }
}