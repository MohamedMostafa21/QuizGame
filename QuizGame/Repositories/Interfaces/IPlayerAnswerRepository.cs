using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IPlayerAnswerRepository : IRepository<PlayerAnswer>
    {
        bool HasAnswered(int gameQuestionId, string userId);
    }
}
