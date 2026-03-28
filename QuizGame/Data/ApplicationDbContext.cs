using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizGame.Models;

namespace QuizGame.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    public DbSet<GameQuestion> GameQuestions { get; set; }
    public DbSet<PlayerAnswer> PlayerAnswers { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Game>()
            .HasOne(g => g.Host)
            .WithMany()
            .HasForeignKey(g => g.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GamePlayer>()
            .HasIndex(e => new { e.GameId, e.UserId })
            .IsUnique();

        builder.Entity<GameQuestion>()
            .HasOne(gq => gq.Winner)
            .WithMany()
            .HasForeignKey(gq => gq.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PlayerAnswer>()
            .HasOne(pa => pa.User)
            .WithMany()
            .HasForeignKey(pa => pa.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PlayerAnswer>()
            .HasOne(pa => pa.GameQuestion)
            .WithMany(gq => gq.PlayerAnswers)
            .HasForeignKey(pa => pa.GameQuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<PlayerAnswer>()
            .HasOne(pa => pa.AnswerOption)
            .WithMany()
            .HasForeignKey(pa => pa.AnswerOptionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}