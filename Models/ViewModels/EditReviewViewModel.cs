using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class EditReviewViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn số điểm đánh giá.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng điền nội dung nhận xét.")]
        public string Comment { get; set; } = string.Empty;
    }
}

