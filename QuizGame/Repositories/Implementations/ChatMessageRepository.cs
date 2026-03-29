using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations;

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    private readonly ApplicationDbContext _context;

    public ChatMessageRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Sync 
    public IEnumerable<ChatMessage> GetByGame(int gameId) =>
        _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.GameId == gameId)
            .OrderBy(m => m.SentAt)
            .ToList();

    // Async
    public Task AddAsync(ChatMessage message)
    {
        Add(message);
        Save();
        return Task.CompletedTask;
    }

    public Task<List<ChatMessage>> GetByGameAsync(int gameId) =>
        _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.GameId == gameId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
}