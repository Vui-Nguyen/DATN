using DATN.Areas.Seller.Models;
using DATN.Services;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DATN.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class SellerProfileController : Controller
    {
        private readonly ISellerService _profileService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SellerProfileController(ISellerService profileService, IWebHostEnvironment webHostEnvironment)
        {
            _profileService = profileService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Xem thông tin profile của Seller đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Challenge();
            }

            var profile = await _profileService.GetProfileByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            var viewModel = new SellerProfileViewModel
            {
                Id = profile.Id,
                UserId = profile.UserId,
                BankAccountNumber = profile.BankAccountNumber,
                BankName = profile.BankName,
                IdentityCardNumber = profile.IdentityCardNumber,
                PortraitImage = profile.PortraitImage,
                FrontIdentityImage = profile.FrontIdentityImage,
                BackIdentityImage = profile.BackIdentityImage,
                Status = profile.Status
            };

            return View(viewModel);
        }

        // Xử lý cập nhật thông tin và upload file ảnh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SellerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var profileEntity = await _profileService.GetProfileByUserIdAsync(model.UserId);
            if (profileEntity == null)
            {
                return NotFound();
            }

            // Thư mục lưu trữ ảnh upload
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "seller");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Xử lý upload ảnh chân dung nếu có file mới
            if (model.PortraitImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PortraitImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PortraitImageFile.CopyToAsync(fileStream);
                }
                profileEntity.PortraitImage = "/uploads/seller/" + uniqueFileName;
            }

            // Xử lý upload mặt trước CCCD
            if (model.FrontIdentityImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.FrontIdentityImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FrontIdentityImageFile.CopyToAsync(fileStream);
                }
                profileEntity.FrontIdentityImage = "/uploads/seller/" + uniqueFileName;
            }

            // Xử lý upload mặt sau CCCD
            if (model.BackIdentityImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.BackIdentityImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.BackIdentityImageFile.CopyToAsync(fileStream);
                }
                profileEntity.BackIdentityImage = "/uploads/seller/" + uniqueFileName;
            }

            // Cập nhật thông tin text
            profileEntity.BankAccountNumber = model.BankAccountNumber;
            profileEntity.BankName = model.BankName;
            profileEntity.IdentityCardNumber = model.IdentityCardNumber;

            var success = await _profileService.UpdateProfileAsync(profileEntity);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật hồ sơ thất bại!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}