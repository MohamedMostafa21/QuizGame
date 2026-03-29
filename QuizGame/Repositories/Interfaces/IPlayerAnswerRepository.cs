using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces;

public interface IPlayerAnswerRepository : IRepository<PlayerAnswer>
{
    // Sync
    bool HasAnswered(int gameQuestionId, string userId);

    // Async
    Task<bool> HasAnsweredAsync(int gameQuestionId, string userId);
    Task AddAsync(PlayerAnswer answer);
}