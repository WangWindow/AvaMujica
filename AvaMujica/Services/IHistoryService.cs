using System.Collections.Generic;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

public interface IHistoryService
{
    Task<List<ChatSession>> GetAllSessionsAsync();
    Task<ChatSession?> GetSessionAsync(string sessionId);
    Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId);
    Task<ChatSession> CreateSessionAsync(string title, string type);
    Task UpdateSessionTitleAsync(string sessionId, string newTitle);
    Task AddMessageAsync(string sessionId, ChatMessage message);
    Task UpdateMessageAsync(ChatMessage message);
    Task<List<ChatSession>> GetChatSessionByTypeAsync(string type);
    Task<List<ChatSessionGroup>> GetChatSessionHistorysByTypeAsync(string type);
}
