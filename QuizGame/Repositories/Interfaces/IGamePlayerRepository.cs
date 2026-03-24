using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IGamePlayerRepository : IRepository<GamePlayer>
    {
        IQueryable<GamePlayer> GetByGame(int gameId);
        GamePlayer? GetByGameAndUser(int gameId, string userId);
        void AddScore(int gameId, string userId, int score);
    }
}
