using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class GameQuestionRepository : Repository<GameQuestion>, IGameQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public GameQuestionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Activate(int id)
        {
            GameQuestion? question = Get(id);

            if(question is not null)
            {
                question.Status = QuestionStatus.Active;
                Update(question);
            }
        }

        public void Close(int id, string winnerId, int points)
        {
            GameQuestion? question = Get(id);

            if(question is not null)
            {
                question.Status = QuestionStatus.Closed;
                question.WinnerId = winnerId;
                Update(question);
            }
        }

        public void CreateBulk(IEnumerable<GameQuestion> gameQuestions)
        {
            _context.GameQuestions.AddRange(gameQuestions);
        }

        public GameQuestion? GetActive(int gameId)
        {
            return _context.GameQuestions.FirstOrDefault(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Active);
        }

        public GameQuestion? GetNextPending(int gameId)
        {
            return _context.GameQuestions.Include(gq => gq.Question).FirstOrDefault(gq => gq.GameId == gameId && gq.Status == QuestionStatus.Pending);
        }

        public GameQuestion? GetWithInnerQuestion(int id)
        {
            return _context.GameQuestions.Include(gq => gq.Question)
                .FirstOrDefault(gq => gq.Id == id);
        }
    }
}
