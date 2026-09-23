using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using DATN.Services.Interfaces;
using DATN.Models.DTOs;

namespace DATN.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinPersonalChannel(string userId)
        {
            string personalGroupName = $"User_{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, personalGroupName);
        }

        public async Task JoinConversation(int conversationId)
        {
            string groupName = $"Conversation_{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveConversation(int conversationId)
        {
            string groupName = $"Conversation_{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task SendMessage(int conversationId, string senderId, string receiverId, string senderName, string text)
        {
            bool isSaved = await _chatService.SaveMessageAsync(conversationId, senderId, text);

            if (isSaved)
            {
                var messageDto = new ChatMessageDto
                {
                    SenderId = int.Parse(senderId),
                    SenderName = senderName,
                    MessageText = text,
                    SentAt = DateTime.UtcNow
                };

                // A. Đẩy tin nhắn vào khung chat cho những ai đang mở phòng chat này
                string chatGroupName = $"Conversation_{conversationId}";
                await Clients.Group(chatGroupName).SendAsync("ReceiveMessage", messageDto);

                // B. Đóng gói dữ liệu thu gọn để cập nhật Sidebar
                var sidebarUpdateData = new
                {
                    ConversationId = conversationId,
                    LastMessage = text,
                    SenderId = int.Parse(senderId),
                    UpdatedAt = DateTime.UtcNow
                };

                await Clients.Group($"User_{receiverId}").SendAsync("UpdateSidebar", sidebarUpdateData);
                await Clients.Group($"User_{senderId}").SendAsync("UpdateSidebar", sidebarUpdateData);
            }
        }
    }
}