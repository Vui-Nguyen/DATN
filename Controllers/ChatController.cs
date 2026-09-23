using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DATN.Services.Interfaces;

namespace DATN.Controllers
{
    [Authorize]
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
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var conversations = await _chatService.GetConversationsByUserAsync(userId, isSeller: false);

            ViewBag.Conversations = conversations;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentConversationId = conversationId;

            if (conversationId.HasValue)
            {
                ViewBag.Messages = await _chatService.GetMessagesAsync(conversationId.Value, page: 1, pageSize: 50);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetOrCreateConversation(int shopId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            // Tìm hoặc tạo mới hội thoại
            int conversationId = await _chatService.GetOrCreateConversationAsync(userId, shopId);

            return RedirectToAction("Index", new { conversationId = conversationId });
        }
    }
}