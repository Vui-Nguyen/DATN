using DATN.Models;
using DATN.Models.Entities;
using DATN.Models.DTOs;
using DATN.Services.Interfaces;
using DATN.Models.ViewModels;
using DATN.Data;

namespace DATN.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
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
                Comment = review.Comment
            };
        }

        public async Task<ServiceResult> CreateAsync(CreateReviewViewModel model)
        {
            var review = new Review
            {
                ProductId = model.ProductId,
                UserId = model.UserId,
                Rating = model.Rating,
                Comment = model.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> UpdateAsync(int id, EditReviewViewModel model)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return new ServiceResult { Success = false, Message = "Đánh giá không tồn tại." };

            review.Rating = model.Rating;
            review.Comment = model.Comment;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return new ServiceResult { Success = false, Message = "Không tìm thấy đánh giá." };

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }
    }
}