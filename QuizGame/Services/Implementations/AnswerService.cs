using QuizGame.Data;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;
using QuizGame.Services.Interfaces;

namespace QuizGame.Services.Implementations;

public class AnswerService : IAnswerService
{
    private readonly ApplicationDbContext _context;
    private readonly IGameQuestionRepository _gameQuestions;
    private readonly IPlayerAnswerRepository _playerAnswers;
    private readonly IGamePlayerRepository _gamePlayers;

    public AnswerService(
        ApplicationDbContext context,
        IGameQuestionRepository gameQuestions,
        IPlayerAnswerRepository playerAnswers,
        IGamePlayerRepository gamePlayers)
    {
        _context = context;
        _gameQuestions = gameQuestions;
        _playerAnswers = playerAnswers;
        _gamePlayers = gamePlayers;
    }

    public async Task<AnswerResult> SubmitAsync(int gameQuestionId, string userId, int answerOptionId)
    {
        var gameQuestion = await _gameQuestions.GetByIdAsync(gameQuestionId);

        if (gameQuestion == null || gameQuestion.Status != QuestionStatus.Active)
            return new AnswerResult { IsRejected = true, Message = "Round is already closed." };

        if (await _playerAnswers.HasAnsweredAsync(gameQuestionId, userId))
            return new AnswerResult { IsRejected = true, Message = "You already answered." };

        var option = await _context.AnswerOptions.FindAsync(answerOptionId);
        if (option == null || option.QuestionId != gameQuestion.QuestionId)
            return new AnswerResult { IsRejected = true, Message = "Invalid answer option." };

        bool isCorrect = option.IsCorrect;
        bool isWinning = false;

        if (isCorrect && gameQuestion.WinnerId == null)
        {
            isWinning = true;
            int points = gameQuestion.Question.Points;

            await _gameQuestions.CloseAsync(gameQuestionId, userId, points);

            await _gamePlayers.AddScoreAsync(gameQuestion.GameId, userId, points);
        }

        await _playerAnswers.AddAsync(new PlayerAnswer
        {
            GameQuestionId = gameQuestionId,
            UserId = userId,
            AnswerOptionId = answerOptionId,
            SubmittedAt = DateTime.UtcNow,
            IsWinningAnswer = isWinning
        });

        var correctId = gameQuestion.Question.AnswerOptions.First(o => o.IsCorrect).Id;

        return new AnswerResult
        {
            IsCorrect = isCorrect,
            IsWinningAnswer = isWinning,
            PointsAwarded = isWinning ? gameQuestion.Question.Points : 0,
            CorrectAnswerOptionId = correctId
        };
    }
}