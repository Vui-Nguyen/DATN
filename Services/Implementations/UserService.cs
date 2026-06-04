using DATN.Models;
using DATN.Services;
using Microsoft.EntityFrameworkCore;
using DATN.Models.Entities;
using System.Threading.Tasks;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;

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

            // Ở đây nên sử dụng BCrypt hoặc Identity để hash mật khẩu, tạm thời minh họa gán trực tiếp
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password, // Cần Hash trước khi lưu thực tế
                RoleId = 2 // Mặc định là Khách hàng (Customer)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Đăng ký thành công." };
        }

        public async Task<UserDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user == null) return null;

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
                RoleName = user.Role?.RoleName
            };
        }

        public async Task<ServiceResult> UpdateProfileAsync(int id, ProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return new ServiceResult { Success = false, Message = "Không tìm thấy người dùng." };

            user.FullName = model.FullName;
            user.Phone = model.Phone;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> ChangePasswordAsync(int id, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.PasswordHash != currentPassword)
                return new ServiceResult { Success = false, Message = "Mật khẩu hiện tại không chính xác." };

            user.PasswordHash = newPassword; // Cần Hash trước khi cập nhật
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
                    RoleName = u.Role != null ? u.Role.RoleName : "Customer"
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
            var user = await _context.Users.FindAsync(id);
            if (user == null) return new ServiceResult { Success = false, Message = "Không tìm thấy tài khoản." };

            // Logic khóa/mở khóa (giả định có thuộc tính IsLocked trong DB)
            // user.IsLocked = !user.IsLocked;

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật trạng thái tài khoản thành công." };
        }

        public async Task<ServiceResult> ChangeRoleAsync(int id, int roleId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return new ServiceResult { Success = false, Message = "Không tìm thấy người dùng." };

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật quyền thành công." };
        }
    }
}