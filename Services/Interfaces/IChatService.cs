using DATN.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services.Interfaces
{
    public interface IChatService
    {
        Task<int> GetOrCreateConversationAsync(string customerId, int shopId);
        Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId, int page, int pageSize);
        Task<bool> SaveMessageAsync(int conversationId, string senderId, string text);
        Task<List<ConversationDto>> GetConversationsByUserAsync(string userId, bool isSeller);
    }
}