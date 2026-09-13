using BCrypt.Net;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
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

            if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
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

            const int sellerRoleId = 2; // Quyền Seller

            // Trường hợp 1: Chuyển từ quyền khác lên Seller
            if (roleId == sellerRoleId && oldRoleId != sellerRoleId)
            {
                // 1. Kiểm tra và xử lý Shop
                var existingShop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == id);
                if (existingShop == null)
                {
                    var newShop = new Shop
                    {
                        UserId = id,
                        ShopName = !string.IsNullOrEmpty(user.FullName) ? $"Cửa hàng của {user.FullName}" : "Cửa hàng mới",
                        Description = "Được khởi tạo tự động khi nâng cấp quyền Seller",
                        IsLocked = false, // 0: Active
                        CreatedAt = DateTime.Now
                    };
                    _context.Shops.Add(newShop);
                }
                else
                {
                    // Nếu đã có shop (từng bị khóa), thì mở khóa lại
                    existingShop.IsLocked = false;

                    // khôi phục lại các sản phẩm khi mở lại shop:
                    var products = await _context.Products.Where(p => p.ShopId == existingShop.ShopId).ToListAsync();
                    foreach (var product in products)
                    {
                        product.IsDeleted = false; // hoặc 0
                    }
                }

                // 2. Kiểm tra và xử lý SellerProfile
                var existingProfile = await _context.SellerProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (existingProfile == null)
                {
                    var newProfile = new SellerProfile
                    {
                        UserId = id,
                        BankAccountNumber = "Chưa cập nhật",
                        BankName = "Chưa cập nhật",
                        IdentityCardNumber = "Chưa cập nhật",
                        PortraitImage = "Chưa cập nhật",
                        FrontIdentityImage = "Chưa cập nhật",
                        BackIdentityImage = "Chưa cập nhật",
                        Status = 1,
                        CreatedAt = DateTime.Now
                    };
                    _context.SellerProfiles.Add(newProfile);
                }
                else
                {
                    existingProfile.Status = 1; 
                }
               
            }
            // Trường hợp 2: Chuyển từ Seller xuống Customer (hoặc quyền khác)
            else if (roleId != sellerRoleId && oldRoleId == sellerRoleId)
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == id);
                if (shop != null)
                {
                    // Chuyển trạng thái shop về 1 (Khóa/Ngừng hoạt động)
                    shop.IsLocked = true;

                    // Lấy tất cả sản phẩm thuộc về shop này và đổi IsDeleted thành 1 (hoặc true)
                    var products = await _context.Products.Where(p => p.ShopId == shop.ShopId).ToListAsync();
                    foreach (var product in products)
                    {
                        product.IsDeleted = true; 
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Cập nhật quyền và đồng bộ cửa hàng thành công." };
        }
        public async Task<bool> AddAddressAsync(AddressViewModel model)
        {
            try
            {

                // Nếu địa chỉ mới được đặt làm mặc định (IsDefault = true)
                // thì cần chuyển các địa chỉ cũ của user này về IsDefault = false
                if (model.IsDefault)
                {
                    var existingAddresses = await _context.Addresses
                        .Where(a => a.UserId == model.UserID && a.IsDefault == true)
                        .ToListAsync();

                    foreach (var addr in existingAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }
                else
                {
                    // Nếu đây là địa chỉ đầu tiên của user, tự động cho nó làm mặc định luôn
                    bool hasAnyAddress = await _context.Addresses.AnyAsync(a => a.UserId == model.UserID);
                    if (!hasAnyAddress)
                    {
                        model.IsDefault = true;
                    }
                }

                // Map từ ViewModel sang Entity của Database
                var newAddress = new Address
                {
                    UserId = model.UserID,
                    ReceiverName = model.ReceiverName,
                    Phone = model.Phone,
                    AddressDetail = model.AddressDetail,
                    IsDefault = model.IsDefault
                };

                // Thêm vào Database
                _context.Addresses.Add(newAddress);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
              
                return false;
            }
        }
        public async Task<List<AddressViewModel>> GetAllAddressesAsync(int userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressViewModel
                {
                    AddressID = a.AddressId,
                    UserID = a.UserId,
                    ReceiverName = a.ReceiverName,
                    Phone = a.Phone,
                    AddressDetail = a.AddressDetail,
                    IsDefault = a.IsDefault ?? false,
                })
                .ToListAsync();

            return addresses;
        }
        public async Task<AddressViewModel?> GetAddressByIdAsync(int addressId, int userId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

            if (address == null) return null;

            return new AddressViewModel
            {
                AddressID = address.AddressId,
                UserID = address.UserId,
                ReceiverName = address.ReceiverName,
                Phone = address.Phone,
                AddressDetail = address.AddressDetail,
                IsDefault = address.IsDefault ?? false,
            };
        }

        public async Task<bool> UpdateAddressAsync(int userId, AddressViewModel model)
        {
            try
            {
                var existingAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == model.AddressID && a.UserId == userId);

                if (existingAddress == null) return false;

                // Nếu chọn làm mặc định, chuyển các địa chỉ cũ về false
                if ((model.IsDefault == true) && (existingAddress.IsDefault != true))
                {
                    var otherAddresses = await _context.Addresses
                        .Where(a => a.UserId == userId && a.IsDefault == true)
                        .ToListAsync();

                    foreach (var addr in otherAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                existingAddress.ReceiverName = model.ReceiverName;
                existingAddress.Phone = model.Phone;
                existingAddress.AddressDetail = model.AddressDetail;
                existingAddress.IsDefault = model.IsDefault;

                _context.Addresses.Update(existingAddress);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}