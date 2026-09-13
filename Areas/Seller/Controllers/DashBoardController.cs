using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]

    public class DashBoardController : Controller
    {

        private readonly ISellerService _sellerService;

        public DashBoardController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }
        public async Task<IActionResult> Index()
        {

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _sellerService.GetDashboardDataAsync(userId);
            if (!result.Success)
            {
                // Có thể redirect về trang thông báo lỗi hoặc trang cập nhật profile
                return View("Error", result.Message);
            }

            return View(result.Data); // Truyền ViewModel sang View
        }
    }
}
