using QuizGame.Models;
using System.Collections.Generic;

namespace QuizGame.Repositories.Interfaces
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        IEnumerable<ChatMessage> GetByGame(int gameId);
    }
}
