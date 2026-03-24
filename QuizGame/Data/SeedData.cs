using Microsoft.EntityFrameworkCore;
using QuizGame.Models;

namespace QuizGame.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (context.Categories.Any()) return;

        var science = new Category { Name = "Science" };
        var history = new Category { Name = "History" };
        context.Categories.AddRange(science, history);
        await context.SaveChangesAsync();

        var questions = new List<Question>
        {
            new Question {
                CategoryId = science.Id,
                Text = "What planet is closest to the Sun?",
                Points = 100, TimeLimitSeconds = 20,
                AnswerOptions = new List<AnswerOption> {
                    new() { Text = "Mercury", IsCorrect = true  },
                    new() { Text = "Venus",   IsCorrect = false },
                    new() { Text = "Earth",   IsCorrect = false },
                    new() { Text = "Mars",    IsCorrect = false },
                }
            },
            new Question {
                CategoryId = science.Id,
                Text = "What is the chemical symbol for water?",
                Points = 100, TimeLimitSeconds = 15,
                AnswerOptions = new List<AnswerOption> {
                    new() { Text = "H2O",  IsCorrect = true  },
                    new() { Text = "CO2",  IsCorrect = false },
                    new() { Text = "NaCl", IsCorrect = false },
                    new() { Text = "O2",   IsCorrect = false },
                }
            },
            new Question {
                CategoryId = history.Id,
                Text = "In what year did World War II end?",
                Points = 200, TimeLimitSeconds = 20,
                AnswerOptions = new List<AnswerOption> {
                    new() { Text = "1945", IsCorrect = true  },
                    new() { Text = "1943", IsCorrect = false },
                    new() { Text = "1947", IsCorrect = false },
                    new() { Text = "1939", IsCorrect = false },
                }
            },
            new Question {
                CategoryId = history.Id,
                Text = "Who was the first US President?",
                Points = 100, TimeLimitSeconds = 15,
                AnswerOptions = new List<AnswerOption> {
                    new() { Text = "George Washington", IsCorrect = true  },
                    new() { Text = "Abraham Lincoln",   IsCorrect = false },
                    new() { Text = "Thomas Jefferson",  IsCorrect = false },
                    new() { Text = "John Adams",        IsCorrect = false },
                }
            },
        };

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }
}