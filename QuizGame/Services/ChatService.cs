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

    
        public IEnumerable<ChatMessage> GetHistoryAsync(int gameId)
        {
            if (gameId <= 0) { 
                throw new ArgumentException("Invalid game ID", nameof(gameId));
            }

            return _chatMessageRepository.GetByGame(gameId);
        }

  
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
