using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Models;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Services
{
    public class ChatService
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public ChatService(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }
        
        /// <summary>
        /// Saves a chat message to the database
        /// </summary>
        /// <param name="gameId">The ID of the game</param>
        /// <param name="userId">The ID of the user sending the message</param>
        /// <param name="text">The message content</param>
        /// <returns>The saved ChatMessage with generated ID and timestamp</returns>
        public ChatMessage SaveAsync(int gameId, string userId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { 
                throw new ArgumentException("Message text cannot be empty", nameof(text));
            }
            var message = new ChatMessage
            {
                GameId = gameId,
                UserId = userId,
                Text = text.Trim(),
                SentAt = DateTime.UtcNow
            };

            _chatMessageRepository.Add(message);
            _chatMessageRepository.Save();

            return message;
        }

        /// <summary>
        /// Retrieves chat history for a specific game, ordered by timestamp
        /// </summary>
        /// <param name="gameId">The ID of the game</param>
        /// <returns>An enumerable of chat messages with user information loaded</returns>
        public IEnumerable<ChatMessage> GetHistoryAsync(int gameId)
        {
            if (gameId <= 0) { 
                throw new ArgumentException("Invalid game ID", nameof(gameId));
            }

            return _chatMessageRepository.GetByGame(gameId);
        }

        /// <summary>
        /// Retrieves chat history for a specific game, with optional limit
        /// </summary>
        /// <param name="gameId">The ID of the game</param>
        /// <param name="maxMessages">Maximum number of messages to return (0 for all)</param>
        /// <returns>An enumerable of chat messages ordered by timestamp</returns>
        public IEnumerable<ChatMessage> GetHistoryAsync(int gameId, int maxMessages)
        {
            var messages = GetHistoryAsync(gameId).ToList();

            if (maxMessages > 0 && messages.Count > maxMessages)
            {
                return messages.Skip(Math.Max(0, messages.Count - maxMessages)).ToList();
            }

            return messages;
        }
    }
}
