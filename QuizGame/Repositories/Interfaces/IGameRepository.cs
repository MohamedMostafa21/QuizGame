using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    // Sync
    Game? GetByRoomCode(string code);
    void UpdateStatus(int id, GameStatus status);
    bool RoomCodeExists(string code);

    // Async
    Task<Game?> GetByIdAsync(int id);
    Task<Game?> GetByRoomCodeAsync(string code);
    Task UpdateStatusAsync(int id, GameStatus status);
    Task<bool> RoomCodeExistsAsync(string code);
}