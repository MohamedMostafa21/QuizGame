using Microsoft.EntityFrameworkCore;
using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations;

public class GameRepository : Repository<Game>, IGameRepository
{
    private readonly ApplicationDbContext _context;

    public GameRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public Game? GetByRoomCode(string code) =>
        _context.Games.FirstOrDefault(g => g.RoomCode == code);

    public bool RoomCodeExists(string code) =>
        _context.Games.Any(g => g.RoomCode == code);

    public void UpdateStatus(int id, GameStatus status)
    {
        Game? game = Get(id);
        if (game is not null)
        {
            game.Status = status;
            Update(game);
        }
    }

    // Gnsh
    public Task<Game?> GetByIdAsync(int id) =>
        _context.Games
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == id);

    public Task<Game?> GetByRoomCodeAsync(string code) =>
        _context.Games
            .Include(g => g.GamePlayers).ThenInclude(gp => gp.User)
            .Include(g => g.Category)
            .FirstOrDefaultAsync(g => g.RoomCode == code);

    public Task UpdateStatusAsync(int id, GameStatus status)
    {
        UpdateStatus(id, status);
        Save();
        return Task.CompletedTask;
    }

    public Task<bool> RoomCodeExistsAsync(string code) =>
        _context.Games.AnyAsync(g => g.RoomCode == code);
}