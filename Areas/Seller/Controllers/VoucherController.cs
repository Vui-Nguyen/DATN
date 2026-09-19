using DATN.Models.DTOs;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DATN.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class VoucherController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        public VoucherController(IVoucherService voucherService, IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _voucherService = voucherService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        private int GetCurrentShopID()
        {
            int userId = 0;
            // Giả sử bạn lưu ShopID trong Claim khi đăng nhập. Hoặc bạn có thể lấy từ Session/DB theo UserId.
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                int.TryParse(userIdClaim.Value, out userId);
            }
            int shopId = _context.Shops.Where(s => s.UserId == userId).Select(s => s.ShopId).FirstOrDefault();
            return shopId;
        }


        // GET: /Voucher/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var pagedDto = await _voucherService.GetAllVouchersAsync(page, pageSize, includeInactive: true);

            var viewModels = pagedDto.Items.Select(v => new VoucherViewModel
            {
                VoucherID = v.VoucherID,
                VoucherCode = v.VoucherCode,
                DiscountPercent = v.DiscountPercent ?? 0,
                StartDate = v.StartDate ?? DateTime.Now,
                EndDate = v.EndDate ?? DateTime.Now,
                Quantity = v.Quantity ?? 0,
                IsActive = v.IsActive,
                ShopId = v.ShopId
            }).ToList();

            var pagedViewModel = new PagedResult<VoucherViewModel>
            {
                Items = viewModels,
                TotalItems = pagedDto.TotalItems,
                CurrentPage = pagedDto.CurrentPage,
                PageSize = pagedDto.PageSize,
                TotalPages = pagedDto.TotalPages
            };

            return View(pagedViewModel);
        }

        // GET: /Voucher/Create
        public IActionResult Create()
        {
            return View(new VoucherViewModel());
        }
       
        // POST: /Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Map Form ViewModel -> DTO
                var dto = new VoucherDto
                {
                    VoucherCode = model.VoucherCode,
                    DiscountPercent = model.DiscountPercent,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Quantity = model.Quantity,
                    ShopId = GetCurrentShopID() // Lấy ShopId từ phương thức GetCurrentShopID
                };

                await _voucherService.CreateVoucherAsync(dto);
                TempData["Success"] = "Thêm mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model); // Trả lại form nếu nhập sai
        }

        // GET: /Voucher/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _voucherService.GetVoucherByIdAsync(id);
            if (dto == null) return NotFound();

            // Map DTO -> Form ViewModel
            var model = new VoucherViewModel
            {
                VoucherID = dto.VoucherID,
                VoucherCode = dto.VoucherCode,
                DiscountPercent = dto.DiscountPercent ?? 0,
                StartDate = dto.StartDate ?? DateTime.Now,
                EndDate = dto.EndDate ?? DateTime.Now,
                Quantity = dto.Quantity ?? 0
            };
            return View(model);
        }

        // POST: /Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VoucherViewModel model)
        {
            if (id != model.VoucherID) return BadRequest();

            if (ModelState.IsValid)
            {
                // Map Form ViewModel -> DTO
                var dto = new VoucherDto
                {
                    VoucherID = model.VoucherID,
                    VoucherCode = model.VoucherCode,
                    DiscountPercent = model.DiscountPercent,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Quantity = model.Quantity
                };

                await _voucherService.UpdateVoucherAsync(dto);
                TempData["Success"] = "Cập nhật mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Voucher/Delete/5
        // Thường dùng HttpPost để bảo mật hơn HttpGet khi thực hiện thao tác xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _voucherService.SoftDeleteVoucherAsync(id);
            if (!success) return NotFound();

            TempData["Success"] = "Đã vô hiệu hóa mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var success = await _voucherService.ActivateVoucherAsync(id);
            if (!success) return NotFound();

            TempData["Success"] = "Đã kích hoạt mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}