using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations;

public class PlayerAnswerRepository : Repository<PlayerAnswer>, IPlayerAnswerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerAnswerRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public bool HasAnswered(int gameQuestionId, string userId) =>
        _context.PlayerAnswers
            .Any(pa => pa.GameQuestionId == gameQuestionId && pa.UserId == userId);

    // Gnsh
    public Task<bool> HasAnsweredAsync(int gameQuestionId, string userId) =>
        _context.PlayerAnswers
            .AnyAsync(pa => pa.GameQuestionId == gameQuestionId && pa.UserId == userId);

    public Task AddAsync(PlayerAnswer answer)
    {
        Add(answer);
        Save();
        return Task.CompletedTask;
    }
}