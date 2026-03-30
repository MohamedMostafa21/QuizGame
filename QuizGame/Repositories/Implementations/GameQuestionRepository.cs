using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations;

public class GameQuestionRepository : Repository<GameQuestion>, IGameQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public GameQuestionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Activate(int id)
    {
        var question = Get(id);
        if (question is not null)
        {
            question.Status = QuestionStatus.Active;
            Update(question);
        }
    }

    public void Close(int id, string winnerId, int points)
    {
        var question = Get(id);
        if (question is not null)
        {
            question.Status = QuestionStatus.Closed;
            question.WinnerId = winnerId;
            Update(question);
        }
    }

    public void CreateBulk(IEnumerable<GameQuestion> gameQuestions)
    {
        _context.GameQuestions.AddRange(gameQuestions);
    }

    public GameQuestion? GetActive(int gameId) =>
        _context.GameQuestions
            .FirstOrDefault(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Active);

    public GameQuestion? GetNextPending(int gameId) =>
        _context.GameQuestions
            .Include(gq => gq.Question)
            .FirstOrDefault(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Pending);

    public GameQuestion? GetWithInnerQuestion(int id) =>
        _context.GameQuestions
            .Include(gq => gq.Question)
            .FirstOrDefault(gq => gq.Id == id);


    public Task<GameQuestion?> GetByIdAsync(int id) =>
        _context.GameQuestions
            .Include(gq => gq.Question)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(gq => gq.Id == id);

    public Task<GameQuestion?> GetActiveAsync(int gameId) =>
        _context.GameQuestions
            .Include(gq => gq.Question)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Active);

    public Task<GameQuestion?> GetNextPendingAsync(int gameId) =>
        _context.GameQuestions
            .Include(gq => gq.Question)
                .ThenInclude(q => q.AnswerOptions)
            .Where(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Pending)
            .OrderBy(gq => gq.Order)
            .FirstOrDefaultAsync();

    public Task CreateBulkAsync(List<GameQuestion> questions)
    {
        CreateBulk(questions);
        Save();
        return Task.CompletedTask;
    }

    public Task ActivateAsync(int id)
    {
        Activate(id);
        Save();
        return Task.CompletedTask;
    }

    public Task CloseAsync(int id, string? winnerId, int pointsAwarded)
    {
        var question = Get(id);
        if (question is not null)
        {
            question.Status = QuestionStatus.Closed;
            question.WinnerId = winnerId;      
            question.PointsAwarded = pointsAwarded;
            Update(question);
            Save();
        }
        return Task.CompletedTask;
    }
}