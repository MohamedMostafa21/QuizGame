using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IGameQuestionRepository : IRepository<GameQuestion>
    {
        void CreateBulk(IEnumerable<GameQuestion> gameQuestions);
        GameQuestion? GetActive(int gameId);
        GameQuestion? GetNextPending(int gameId);
        void Activate(int id);
        void Close(int id, string winnerId, int points);
    }
}
