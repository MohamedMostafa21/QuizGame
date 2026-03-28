using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class AnswerOptionRepository : Repository<AnswerOption>, IAnswerOptionRepository
    {
        private readonly ApplicationDbContext _context;

        public AnswerOptionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AnswerOption> GetByQuestionId(int id)
        {
            return _context.AnswerOptions.Where(o => o.QuestionId == id).ToList();
        }
    }
}
