namespace QuizGame.Models;

public enum GameStatus
{
    Waiting = 0,
    InProgress = 1,
    Finished = 2
}

public enum QuestionStatus
{
    Pending = 0,
    Active = 1,
    Closed = 2
}

public enum JoinGameResult
{
    Success = 0,
    GameNotFound = 1,
    GameInProgress = 2,
}