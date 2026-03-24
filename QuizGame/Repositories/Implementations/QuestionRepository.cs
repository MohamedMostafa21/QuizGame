using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        private readonly ApplicationDbContext _context;
        
        public QuestionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        
        public IQueryable<Question> GetRandomSample(int categoryId, int count)
        {
            return _context.Questions
                .Where(q => q.CategoryId == categoryId)
                .OrderBy(q => Guid.NewGuid())
                .Take(count);
        }
    }
}
