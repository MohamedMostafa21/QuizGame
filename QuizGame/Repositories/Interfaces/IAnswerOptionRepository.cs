using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IAnswerOptionRepository : IRepository<AnswerOption>
    {
        List<AnswerOption> GetByQuestionId(int id);
    }
}
