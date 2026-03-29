using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces;

public interface IGamePlayerRepository : IRepository<GamePlayer>
{
    // Sync
    IQueryable<GamePlayer> GetByGame(int gameId);
    GamePlayer? GetByGameAndUser(int gameId, string userId);
    void AddScore(int gameId, string userId, int score);
    void RemovePlayer(int gameId, string userId);

    // Async
    Task<List<GamePlayer>> GetByGameAsync(int gameId);
    Task<GamePlayer?> GetByGameAndUserAsync(int gameId, string userId);
    Task AddAsync(GamePlayer gp);
    Task AddScoreAsync(int gameId, string userId, int points);
    Task<bool> IsUserInGameAsync(int gameId, string userId);
}