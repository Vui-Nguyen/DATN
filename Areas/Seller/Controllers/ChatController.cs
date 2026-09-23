using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DATN.Services.Interfaces;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<IActionResult> Index(int? conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { area = "" });

            var conversations = await _chatService.GetConversationsByUserAsync(userId, isSeller: true);

            ViewBag.Conversations = conversations;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentConversationId = conversationId;

            // Lấy lịch sử chat của hội thoại đang được chọn
            if (conversationId.HasValue)
            {
                ViewBag.Messages = await _chatService.GetMessagesAsync(conversationId.Value, page: 1, pageSize: 50);
            }

            return View();
        }
    }
}