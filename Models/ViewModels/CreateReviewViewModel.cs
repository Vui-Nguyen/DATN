using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class CreateReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số điểm đánh giá.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng viết nội dung nhận xét.")]
        [StringLength(500, ErrorMessage = "Nội dung đánh giá không được dài quá 500 ký tự.")]
        public string Comment { get; set; } = string.Empty;
    }
}