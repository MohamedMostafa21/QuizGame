using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces;

public interface IGameQuestionRepository : IRepository<GameQuestion>
{
    // Sync
    void CreateBulk(IEnumerable<GameQuestion> gameQuestions);
    GameQuestion? GetActive(int gameId);
    GameQuestion? GetNextPending(int gameId);
    void Activate(int id);
    void Close(int id, string winnerId, int points);
    GameQuestion? GetWithInnerQuestion(int id);

    // Async
    Task<GameQuestion?> GetByIdAsync(int id);
    Task<GameQuestion?> GetActiveAsync(int gameId);
    Task<GameQuestion?> GetNextPendingAsync(int gameId);
    Task CreateBulkAsync(List<GameQuestion> questions);
    Task ActivateAsync(int id);
    Task CloseAsync(int id, string? winnerId, int pointsAwarded);
}