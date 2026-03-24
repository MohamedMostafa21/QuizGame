using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class PlayerAnswerRepository : Repository<PlayerAnswer>, IPlayerAnswerRepository
    {
        private readonly ApplicationDbContext _context;

        public PlayerAnswerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public bool HasAnswered(int gameQuestionId, string userId)
        {
            return _context.PlayerAnswers.Any(pa => pa.GameQuestionId == gameQuestionId && pa.UserId == userId);
        }
    }
}
