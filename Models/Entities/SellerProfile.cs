using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models.Entities 
{
    public class SellerProfile
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Liên kết với bảng User

        [Required, MaxLength(50)]
        public string BankAccountNumber { get; set; } // Số tài khoản

        [Required, MaxLength(100)]
        public string BankName { get; set; } // Tên ngân hàng

        [Required, MaxLength(20)]
        public string IdentityCardNumber { get; set; } // Số CCCD

        [Required]
        public string PortraitImage { get; set; } // Đường dẫn ảnh chân dung

        [Required]
        public string FrontIdentityImage { get; set; } // Đường dẫn mặt trước CCCD

        [Required]
        public string BackIdentityImage { get; set; } // Đường dẫn mặt sau CCCD

        // Trạng thái duyệt: 0 = Chờ duyệt, 1 = Đã duyệt, 2 = Bị từ chối
        public int Status { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}