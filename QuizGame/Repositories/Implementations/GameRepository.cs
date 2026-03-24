using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class GameRepository : Repository<Game>, IGameRepository
    {
        private readonly ApplicationDbContext _context;

        public GameRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Game? GetByRoomCode(string code)
        {
            return _context.Games.FirstOrDefault(g => g.RoomCode == code);
        }

        public bool RoomCodeExists(string code)
        {
            return _context.Games.Any(g => g.RoomCode == code);
        }

        public void UpdateStatus(int id, GameStatus status)
        {
            Game? game = Get(id);

            if(game is not null)
            {
                game.Status = status;
                Update(game);
            }
        }
    }
}
