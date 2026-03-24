using Microsoft.AspNetCore.Identity;

namespace QuizGame.Models;

public class User : IdentityUser
{
    public int TotalScore { get; set; } = 0;

    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}