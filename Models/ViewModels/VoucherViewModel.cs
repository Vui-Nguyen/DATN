using Microsoft.VisualStudio.Web.CodeGenerators.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models.ViewModels
{
    public class VoucherViewModel
    {
        // Dùng cho: Xem chi tiết, Sửa, Xóa (Bỏ [Required] ở đây nếu dùng cho form Thêm vì lúc thêm chưa có ID)
        public int VoucherID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã voucher.")]
        [StringLength(50, ErrorMessage = "Mã voucher không được vượt quá 50 ký tự.")]
        public string VoucherCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm giá.")]
        [Range(0.01, 100, ErrorMessage = "Phần trăm giảm giá phải từ lớn hơn 0 đến 100.")]
        public int DiscountPercent { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 1.")]
        public int Quantity { get; set; }

        // Trạng thái (Phục vụ cho việc hiển thị ở phần Xem danh sách/Chi tiết)
        public bool IsActive { get; set; }
        public int ShopId { get; set; }
        public string StatusLabel => IsActive ? "Hoạt động" : "Đã khóa";
        // Thông báo phụ trợ khi thực hiện xóa (Phục vụ cho popup/trang xác nhận xóa)
        public string ConfirmationMessage => $"Bạn có chắc chắn muốn xóa mã giảm giá [{VoucherCode}] này không?";
    }
}