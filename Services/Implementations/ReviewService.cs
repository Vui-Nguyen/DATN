using DATN.Areas.Seller.Models.DTOs;
using DATN.Data;
using DATN.Models;
using DATN.Models.DTOs;
using DATN.Models.Entities;
using DATN.Models.ViewModels;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<OrderItemDto>> GetDeliveredProductsByUserIdAsync(int userId, int page, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Delivered")
                .SelectMany(o => o.OrderItems)
                .Join(_context.ProductVariants,
                    orderItem => orderItem.VariantId,
                    variant => variant.VariantId,
                    (orderItem, variant) => new { orderItem, variant })
                .Join(_context.Products,
                    x => x.variant.ProductId,
                    product => product.ProductId,
                    (x, product) => new
                    {
                        x.orderItem.OrderItemId,
                        x.orderItem.VariantId,
                        ProductId = product.ProductId,
                        ProductName = $"{product.ProductName} - {x.variant.VariantName}",
                        x.orderItem.Quantity,
                        Price = x.orderItem.UnitPrice,
                        ExistingReview = _context.Reviews
                            .FirstOrDefault(r => r.UserId == userId && r.OrderItemId == x.orderItem.OrderItemId)
                    })
                .Select(item => new OrderItemDto
                {
                    OrderItemId = item.OrderItemId, 
                    VariantID = item.VariantId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    HasReviewed = item.ExistingReview != null,
                    ReviewId = item.ExistingReview != null ? (int?)item.ExistingReview.ReviewId : null,
                    Reply = item.ExistingReview != null ? item.ExistingReview.Reply : null,
                    RepliedAt = item.ExistingReview != null ? item.ExistingReview.RepliedAt : null
                });

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderItemDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };
        }
        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return null;

            return new ReviewDto
            {
                ReviewID = review.ReviewId,
                ProductID = review.ProductId,
                UserID = review.UserId,
                Rating =(int) review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt ?? DateTime.Now,
                Reply = review.Reply,
                RepliedAt = review.RepliedAt
            };
        }

        public async Task<ServiceResult> CreateAsync(CreateReviewViewModel model)
        {
            try
            {
                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = model.UserId,
                    OrderItemId = model.OrderItemId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Đánh giá thành công." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = "Lỗi khi lưu đánh giá: " + ex.Message };
            }
        }
        public async Task<ServiceResult> UpdateAsync(int id, EditReviewViewModel model)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                    return new ServiceResult { Success = false, Message = "Không tìm thấy đánh giá cần sửa." };

                review.Rating = model.Rating;
                review.Comment = model.Comment;

                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Cập nhật thành công." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message };
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }



        //Seller
        public async Task<PagedResult<ReviewSellerDto>> GetReviewsByShopIdAsync(int shopId, int page, int pageSize)
        {
            // 1. Truy vấn các đánh giá thuộc các sản phẩm của ShopID tương ứng
            var query = _context.Reviews
                .Join(_context.Products,
                      review => review.ProductId,
                      product => product.ProductId,
                      (review, product) => new { review, product })
                .Where(x => x.product.ShopId == shopId)
                .Join(_context.Users,
                      x => x.review.UserId,
                      user => user.UserId,
                      (x, user) => new ReviewSellerDto
                      {
                          ReviewId = x.review.ReviewId,
                          ProductId = x.product.ProductId,
                          ProductName = x.product.ProductName,
                          UserId = x.review.UserId,
                          Email = user.Email,
                          Rating = x.review.Rating ?? 0,
                          Comment = x.review.Comment,
                          CreatedAt = x.review.CreatedAt ?? DateTime.Now,
                          Reply = x.review.Reply,
                          RepliedAt = x.review.RepliedAt

                      })
                .OrderByDescending(r => r.CreatedAt);

            // 2. Tính toán phân trang (PagedResult)
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReviewSellerDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };
        }
        public async Task<ServiceResult> ReplyReviewAsync(int reviewId, string replyContent, int shopId)
        {
            var review = await _context.Reviews
                .Join(_context.Products, r => r.ProductId, p => p.ProductId, (r, p) => new { r, p })
                .Where(x => x.r.ReviewId == reviewId && x.p.ShopId == shopId) // Bảo mật: Chỉ seller sở hữu sản phẩm mới được trả lời
                .Select(x => x.r)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return new ServiceResult { Success = false, Message = "Không tìm thấy đánh giá hoặc bạn không có quyền phản hồi." };
            }

            review.Reply = replyContent;
            review.RepliedAt = DateTime.Now;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Phản hồi đánh giá thành công." };
        }
        public async Task<bool> DeleteBySellerAsync(int reviewId, int shopId)
        {
            var review = await _context.Reviews
                .Join(_context.Products, r => r.ProductId, p => p.ProductId, (r, p) => new { r, p })
                .Where(x => x.r.ReviewId == reviewId && x.p.ShopId == shopId)
                .Select(x => x.r)
                .FirstOrDefaultAsync();

            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ServiceResult> UpdateReplyAsync(int reviewId, int sellerShopId, string newReplyContent)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.Product) // Bao gồm sản phẩm để check xem thuộc shop nào
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

                if (review == null)
                {
                    return new ServiceResult { Success = false, Message = "Không tìm thấy đánh giá." };
                }

                // Kiểm tra xem sản phẩm của đánh giá này có thực sự thuộc về shop của seller đang đăng nhập hay không (Bảo mật)
                if (review.Product.ShopId != sellerShopId)
                {
                    return new ServiceResult { Success = false, Message = "Bạn không có quyền sửa phản hồi này." };
                }

                // Cập nhật nội dung phản hồi mới và thời gian sửa
                review.Reply = newReplyContent;
                review.RepliedAt = DateTime.Now; // Hoặc bạn có thể tạo thêm trường UpdatedAt nếu muốn

                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Cập nhật phản hồi thành công." };
            }
            catch (Exception)
            {
                return new ServiceResult { Success = false, Message = "Lỗi hệ thống khi cập nhật phản hồi." };
            }
        }
    }
}