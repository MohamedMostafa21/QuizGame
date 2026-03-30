using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations;

public class GamePlayerRepository : Repository<GamePlayer>, IGamePlayerRepository
{
    private readonly ApplicationDbContext _context;

    public GamePlayerRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<GamePlayer> GetByGame(int gameId) =>
        _context.GamePlayers.Where(gp => gp.GameId == gameId);

    public GamePlayer? GetByGameAndUser(int gameId, string userId) =>
        _context.GamePlayers
            .FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);

    public void AddScore(int gameId, string userId, int score)
    {
        var player = _context.GamePlayers
            .FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);
        if (player is not null)
            player.Score += score;
    }

    public void RemovePlayer(int gameId, string userId)
    {
        var player = _context.GamePlayers
            .FirstOrDefault(gp => gp.GameId == gameId && gp.UserId == userId);
        if (player is not null)
            _context.GamePlayers.Remove(player);
    }

    public Task<List<GamePlayer>> GetByGameAsync(int gameId) =>
        _context.GamePlayers
            .Include(gp => gp.User)
            .Where(gp => gp.GameId == gameId)
            .ToListAsync();

    public Task<GamePlayer?> GetByGameAndUserAsync(int gameId, string userId) =>
        _context.GamePlayers
            .FirstOrDefaultAsync(gp => gp.GameId == gameId && gp.UserId == userId);

    public Task AddAsync(GamePlayer gp)
    {
        Add(gp);
        Save();
        return Task.CompletedTask;
    }

    public Task AddScoreAsync(int gameId, string userId, int points)
    {
        AddScore(gameId, userId, points);
        Save();
        return Task.CompletedTask;
    }

    public Task<bool> IsUserInGameAsync(int gameId, string userId) =>
        _context.GamePlayers
            .AnyAsync(gp => gp.GameId == gameId && gp.UserId == userId);
}