using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DATN.Data;

namespace DATN.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetOrCreateConversationAsync(string customerId, int shopId)
        {
            int parsedCustomerId = int.Parse(customerId);

            // Tìm hội thoại đã tồn tại
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.CustomerId == parsedCustomerId && c.ShopId == shopId);

            // Nếu chưa có, tạo mới
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    CustomerId = parsedCustomerId,
                    ShopId = shopId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversation.ConversationId;
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId, int page, int pageSize)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                // Sắp xếp giảm dần để lấy tin nhắn mới nhất, sau đó đảo ngược lại để hiển thị từ trên xuống dưới
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ChatMessageDto
                {
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    MessageText = m.MessageText,
                    SentAt = m.SentAt
                }).ToListAsync();

            messages.Reverse();
            return messages;
        }

        public async Task<bool> SaveMessageAsync(int conversationId, string senderId, string text)
        {
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = int.Parse(senderId),
                MessageText = text,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(message);

            // Cập nhật lại thời gian UpdatedAt của Conversation để đưa lên đầu danh sách
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.UpdatedAt = DateTime.Now;
            }

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<ConversationDto>> GetConversationsByUserAsync(string userId, bool isSeller)
        {
            int parsedUserId = int.Parse(userId);

            var query = _context.Conversations.AsQueryable();

            if (isSeller)
            {
                // Lấy các hội thoại thuộc về (các) Shop mà User này sở hữu
                query = query.Where(c => c.Shop.UserId == parsedUserId);
            }
            else
            {
                // Lấy các hội thoại mà User này là khách hàng
                query = query.Where(c => c.CustomerId == parsedUserId);
            }

            return await query
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    // Nếu là Seller thì Partner là Khách (Customer), ngược lại Partner là Shop
                    PartnerName = isSeller ? c.Customer.FullName : c.Shop.ShopName,
                    PartnerAvatar = isSeller ? null : c.Shop.AvatarShop,

                    LastMessage = c.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.MessageText)
                        .FirstOrDefault(),

                    UnreadCount = c.Messages
                        .Count(m => !m.IsRead && m.SenderId != parsedUserId)
                })
                .ToListAsync();
        }
    }
}