using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services;
using BCrypt.Net;
using DATN.Services.Interfaces;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DATN.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel model)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (exists)
                return new ServiceResult { Success = false, Message = "Email này đã được đăng ký sử dụng." };

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = hashedPassword, 
                RoleId = 3 // Mặc định là Khách hàng (Customer)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Đăng ký thành công." };
        }

        public async Task<UserDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email == email);


            if (user == null) return null;

            bool isPasswordValid = false;


            if (user.PasswordHash.StartsWith("$2a$") || user.PasswordHash.StartsWith("$2b$") || user.PasswordHash.StartsWith("$2y$"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            else
            {
                isPasswordValid = (password == user.PasswordHash);

                if (isPasswordValid)
                {
                    //  Tự động nâng cấp mật khẩu sang BCrypt ngay khi user đăng nhập thành công
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    await _context.SaveChangesAsync();
                }
            }

            if (!isPasswordValid) return null;

            return new UserDto
            {
                UserID = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role?.RoleName
            };
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return null;

            return new UserDto
            {
                UserID = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleName = user.Role?.RoleName,
                IsLocked = user.IsLocked
            };
        }

        public async Task<ServiceResult> UpdateProfileAsync(int id, ProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ServiceResult { Success = false, Message = "Không tìm thấy người dùng." };

            user.FullName = model.FullName;
            user.Phone = model.Phone;

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                // Bắt lỗi khi lưu DB (ví dụ: tràn dữ liệu, lỗi ràng buộc, mất kết nối...)
                return new ServiceResult { Success = false, Message = $"Lỗi khi lưu cơ sở dữ liệu: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> ChangePasswordAsync(int id, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null ||!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return new ServiceResult { Success = false, Message = "Mật khẩu hiện tại không chính xác." };
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordHash = newPasswordHash;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true };
        }

        public async Task<PagedResult<UserDto>> GetAllAsync(int page, string? keyword)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new UserDto
                {
                    UserID = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role != null ? u.Role.RoleName : "Customer",
                    IsLocked = u.IsLocked
                }).ToListAsync();

            return new PagedResult<UserDto>
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<ServiceResult> ToggleLockAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy tài khoản." };
                }

                // Đảo ngược trạng thái: đang khóa thì mở, đang mở thì khóa
                user.IsLocked = user.IsLocked ? false : true;

                await _context.SaveChangesAsync();

                string actionMessage = !user.IsLocked ? "Mở khóa tài khoản thành công." : "Khóa tài khoản thành công.";

                return new ServiceResult { Success = true, Message = actionMessage };
            }
            catch (Exception)
            {
                return new ServiceResult { Success = false, Message = "Lỗi hệ thống khi cập nhật trạng thái." };
            }
        }

        public async Task<ServiceResult> ChangeRoleAsync(int id, int roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return new ServiceResult { Success = false, Message = "Không tìm thấy người dùng." };

            int oldRoleId = user.RoleId;
            user.RoleId = roleId;

            // Giả sử RoleId = 2 là quyền Seller (theo quy ước trước đó của bạn)
            const int sellerRoleId = 2;

            // Trường hợp 1: Chuyển từ quyền khác lên Seller
            if (roleId == sellerRoleId && oldRoleId != sellerRoleId)
            {
                bool shopExists = await _context.Shops.AnyAsync(s => s.UserId == id);
                if (!shopExists)
                {
                    var newShop = new Shop
                    {
                        UserId = id,
                        ShopName = !string.IsNullOrEmpty(user.FullName) ? $"Cửa hàng của {user.FullName}" : "Cửa hàng mới",
                        Description = "Được khởi tạo tự động khi nâng cấp quyền Seller",
                        CreatedAt = DateTime.Now
                    };
                    _context.Shops.Add(newShop);
                }
            }
            // Trường hợp 2: Chuyển từ Seller xuống Customer (hoặc quyền khác)
            else if (roleId != sellerRoleId && oldRoleId == sellerRoleId)
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == id);
                if (shop != null)
                {
                    // Xóa Shop khỏi bảng Shops (Lưu ý: Nếu shop đã có sản phẩm/đơn hàng, cần đảm bảo DB có thiết lập Cascade Delete hoặc xóa dữ liệu liên quan trước)
                    _context.Shops.Remove(shop);
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật quyền và đồng bộ cửa hàng thành công." };
        }
    }
}