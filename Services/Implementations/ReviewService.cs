using DATN.Models;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;
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
                          x.orderItem.VariantId,
                          ProductName = $"{product.ProductName} - {x.variant.VariantName}",
                          x.orderItem.Quantity,
                          Price = x.orderItem.UnitPrice,
                          ExistingReview = _context.Reviews
                      .FirstOrDefault(r => r.UserId == userId && r.ProductId == x.variant.ProductId)
                      })
                .Select(item => new OrderItemDto
                {
                    VariantID = item.VariantId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    HasReviewed = item.ExistingReview != null,
                    ReviewId = item.ExistingReview != null ? (int?)item.ExistingReview.ReviewId : null 
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
    }
}