using DATN.Data;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implementations
{
    public class SellerService : ISellerService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SellerService(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<ServiceResult> ApproveSellerAsync(int id)
        {
            try
            {
                var profile = await _context.SellerProfiles
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (profile == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy hồ sơ người bán." };
                }

                // 1. Cập nhật trạng thái hồ sơ thành đã duyệt
                profile.Status = 1;

                // 2. Nâng quyền User lên Seller (RoleID = 2)
                if (profile.User != null)
                {
                    profile.User.RoleId = 2;
                }

                // 3. Tự động khởi tạo Shop mới gắn với UserID này
                var shop = new Shop
                {
                    UserId = profile.UserId,
                    ShopName = profile.User?.FullName,
                    Description = "Cửa hàng mới đăng ký",
                    CreatedAt = DateTime.Now
                };

                _context.Shops.Add(shop);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = $"Đã duyệt tài khoản {profile.User?.FullName} thành công!" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = "Lỗi khi duyệt seller: " + ex.Message };
            }
        }
        public async Task<ServiceResult> RegisterSellerAsync(int userId, RegisterSellerViewModel model)
        {
            try
            {
                // 1. Kiểm tra xem người này đã gửi yêu cầu trước đó chưa
                bool hasRequested = await _context.SellerProfiles.AnyAsync(s => s.UserId == userId);
                // 1. Tìm yêu cầu đăng ký cũ của người dùng này
                var existingRequest = await _context.SellerProfiles
                                                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (existingRequest != null)
                {
                    // Kiểm tra xem yêu cầu đã được tạo quá 30 ngày chưa
                    var daysSinceRequest = (DateTime.Now - existingRequest.CreatedAt).TotalDays;

                    if (daysSinceRequest >= 30)
                    {
                        // Nếu đã đủ 30 ngày, xóa bản ghi cũ đi
                        _context.SellerProfiles.Remove(existingRequest);
                        await _context.SaveChangesAsync();

                    }
                    else
                    {
                        // Nếu chưa đủ 30 ngày, chặn lại
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Bạn đã gửi yêu cầu trước đó. Vui lòng thử lại sau {30 - Math.Floor(daysSinceRequest)} ngày nữa."
                        };
                    }
                }

                    // 2. Upload 3 file ảnh lên server và lấy đường dẫn
                string portraitUrl = await UploadImageAsync(model.PortraitImage);
                string frontIdUrl = await UploadImageAsync(model.FrontIdentityImage);
                string backIdUrl = await UploadImageAsync(model.BackIdentityImage);

                // 3. Tạo đối tượng SellerProfile để lưu vào Database
                var sellerProfile = new SellerProfile
                {
                    UserId = userId,
                    BankName = model.BankName,
                    BankAccountNumber = model.BankAccountNumber,
                    IdentityCardNumber = model.IdentityCardNumber,
                    PortraitImage = portraitUrl,
                    FrontIdentityImage = frontIdUrl,
                    BackIdentityImage = backIdUrl,
                    Status = 0
                };

                // 4. Cập nhật lại Họ tên và SĐT vào bảng User
                var currentUser = await _context.Users.FindAsync(userId);
                if (currentUser != null)
                {
                    currentUser.FullName = model.FullName;
                    currentUser.Phone = model.Phone;
                    _context.Users.Update(currentUser);
                }

                // 5. Lưu vào Database
                _context.SellerProfiles.Add(sellerProfile);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Gửi yêu cầu đăng ký Seller thành công. Vui lòng chờ quản trị viên phê duyệt." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = "Có lỗi xảy ra trong quá trình xử lý: " + ex.Message };
            }
        }

        // Hàm hỗ trợ upload ảnh (giữ nguyên logic cũ của bạn)
        private async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "sellers");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/sellers/" + uniqueFileName;
        }
        public async Task<SellerProfile?> GetProfileByUserIdAsync(int userId)
        {
            return await _context.SellerProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Sửa thông tin profile
        public async Task<bool> UpdateProfileAsync(SellerProfile model)
        {
            var existingProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (existingProfile == null)
            {
                return false;
            }

            // Cập nhật các trường thông tin cần thiết
            existingProfile.BankAccountNumber = model.BankAccountNumber;
            existingProfile.BankName = model.BankName;
            existingProfile.IdentityCardNumber = model.IdentityCardNumber;

            // Nếu có cập nhật ảnh mới thì gán đường dẫn mới, nếu không giữ nguyên ảnh cũ
            if (!string.IsNullOrEmpty(model.PortraitImage))
                existingProfile.PortraitImage = model.PortraitImage;

            if (!string.IsNullOrEmpty(model.FrontIdentityImage))
                existingProfile.FrontIdentityImage = model.FrontIdentityImage;

            if (!string.IsNullOrEmpty(model.BackIdentityImage))
                existingProfile.BackIdentityImage = model.BackIdentityImage;

            // Khi shop sửa lại thông tin, có thể reset trạng thái về Chờ duyệt (0) nếu cần thiết
            // existingProfile.Status = 0; 

            _context.SellerProfiles.Update(existingProfile);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
