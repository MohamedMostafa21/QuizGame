using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class GamePlayerRepository : Repository<GamePlayer>, IGamePlayerRepository
    {
        private readonly ApplicationDbContext _context;

        public GamePlayerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddScore(int gameId, string userId, int score)
        {
            GamePlayer? player = _context.GamePlayers.FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);

            if (player is not null)
                player.Score += score;
        }

        public IQueryable<GamePlayer> GetByGame(int gameId)
        {
            return _context.GamePlayers.Where(gp => gp.GameId == gameId);
        }

        public GamePlayer? GetByGameAndUser(int gameId, string userId)
        {
            return _context.GamePlayers.FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);
        }

        public void RemovePlayer(int gameId, string userId)
        {
            GamePlayer? player = _context.GamePlayers
                .FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);

            if (player is not null)
                _context.GamePlayers.Remove(player);
        }
    }
}
