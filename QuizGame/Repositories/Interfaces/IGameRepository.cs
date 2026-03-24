using QuizGame.Models;

namespace QuizGame.Repositories.Interfaces
{
    public interface IGameRepository : IRepository<Game>
    {
        Game? GetByRoomCode(string code);
        void UpdateStatus(int id, GameStatus status);
        bool RoomCodeExists(string code);
    }
}
