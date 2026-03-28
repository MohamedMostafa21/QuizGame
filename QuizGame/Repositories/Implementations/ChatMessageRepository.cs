using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<ChatMessage> GetByGame(int gameId)
        {
            return _context.ChatMessages
                .Where(cm => cm.GameId == gameId)
                .OrderBy(cm => cm.SentAt)
                .Include(cm => cm.User)
                .ToList();
        }
    }
}
