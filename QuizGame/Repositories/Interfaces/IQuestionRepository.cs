using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IQuestionRepository : IRepository<Question>
    {
        IQueryable<Question> GetRandomSample(int? categoryId, int count);
    }
}
