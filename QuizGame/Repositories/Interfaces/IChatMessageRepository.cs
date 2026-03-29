using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    // Sync
    IEnumerable<ChatMessage> GetByGame(int gameId);

    // Async
    Task AddAsync(ChatMessage message);
    Task<List<ChatMessage>> GetByGameAsync(int gameId);
}