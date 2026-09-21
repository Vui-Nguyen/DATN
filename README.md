# 🛒 Multi-Vendor E-Commerce System (Hệ thống Sàn Thương Mại Điện Tử Đa Người Bán)

Dự án website thương mại điện tử đa người bán được xây dựng trên nền tảng **ASP.NET Core MVC**, áp dụng kiến trúc phân tầng (Layered Architecture), Dependency Injection và tích hợp cổng thanh toán trực tuyến VNPay.

---

## 📌 Tính năng chính

### 1. Phân hệ Khách hàng (Customer)

- **Khám phá sản phẩm:** Tìm kiếm, lọc sản phẩm theo danh mục và thương hiệu.
- **Chi tiết sản phẩm:** Xem thông tin, chọn biến thể (màu sắc, kích thước) và xem đánh giá.
- **Giỏ hàng & Đặt hàng:** Thêm/sửa/xóa giỏ hàng, áp dụng mã giảm giá (Voucher).
- **Thanh toán:** Tích hợp cổng thanh toán trực tuyến qua **VNPay** và phương thức COD.
- **Quản lý đơn hàng:** Theo dõi lịch sử đơn hàng và cập nhật trạng thái đơn chi tiết.
- **Đánh giá:** Gửi nhận xét và chấm điểm sản phẩm sau khi mua.

### 2. Phân hệ Người bán (Seller Area)

- **Đăng ký gian hàng:** Gửi hồ sơ đăng ký trở thành Seller.
- **Dashboard thống kê:** Tổng quan doanh thu, trạng thái đơn hàng và sản phẩm.
- **Quản lý sản phẩm:** CRUD sản phẩm, cấu hình biến thể (Variant) và số lượng kho.
- **Quản lý đơn hàng:** Tiếp nhận, xác nhận và cập nhật tiến trình xử lý đơn.
- **Quản lý Voucher & Shop:** Tạo mã giảm giá riêng cho cửa hàng và tùy chỉnh hồ sơ shop.

### 3. Phân hệ Quản trị viên (Admin Area)

- **Kiểm duyệt Seller:** Phê duyệt hoặc từ chối các yêu cầu mở gian hàng mới.
- **Quản trị danh mục & thương hiệu:** Quản lý toàn bộ Categories và Brands trên hệ thống.
- **Quản trị người dùng:** Quản lý danh sách tài khoản, khóa hoặc phân quyền hệ thống.

---

## 🛠️ Công nghệ sử dụng

- **Backend:** ASP.NET Core MVC, C#, .NET
- **ORM:** Entity Framework Core (Code-First)
- **Database:** Microsoft SQL Server
- **Frontend:** Razor Pages/Views, HTML5, CSS3, JavaScript, Bootstrap
- **Thanh toán:** Cổng thanh toán VNPay API
- **Kiến trúc & Kỹ thuật:**
  - Clean / Layered Architecture (Entities, DTOs, ViewModels, Service Layer)
  - Dependency Injection (DI)
  - ASP.NET Core Areas
  - Generic Result Pattern (`ServiceResult`, `PagedResult`)
  - Database Transactions (`IDbContextTransaction`)

---

## 📂 Cấu trúc dự án

```text
DATN/
├── Areas/
│   ├── Admin/              # Controllers, Models, Views dành cho Quản trị viên
│   └── Seller/             # Controllers, Models, Views dành cho Người bán
├── Controllers/            # Controllers chính cho khách hàng & hệ sinh thái chung
├── Data/                   # AppDbContext, cấu hình Entity Framework & Migrations
├── Helper/                 # Tiện ích bổ trợ (VnPayLibrary,...)
├── Models/
│   ├── DTOs/               # Data Transfer Objects
│   ├── Entities/           # Lớp thực thể CSDL (Product, Order, Variant,...)
│   └── ViewModels/         # Models truyền tải dữ liệu hiển thị giao diện
├── Services/
│   ├── Interfaces/         # Interface định nghĩa nghiệp vụ
│   └── Implementations/    # Triển khai chi tiết nghiệp vụ (Business Logic)
├── Views/                  # Giao diện Razor Views & Layouts
└── wwwroot/                # Tài nguyên tĩnh (CSS, JS, Images, Libs)
```

🚀 Hướng dẫn cài đặt & Chạy ứng dụng
Yêu cầu môi trường
.NET SDK (phiên bản tương thích với dự án)

Visual Studio hoặc Visual Studio Code

SQL Server (LocalDB, SQL Express hoặc SQL Server Management Studio)

Các bước thiết lập
Clone repository về máy:

Bash
git clone <git clone [text](https://github.com/Vui-Nguyen/DATN)

> cd DATN
> Cấu hình chuỗi kết nối CSDL và VNPay:

Mở file appsettings.json (hoặc tạo appsettings.Local.json) và chỉnh sửa chuỗi kết nối:

JSON
{
"ConnectionStrings": {
"DefaultConnection": "Server=.;Database=MultiVendorDb;Trusted_Connection=True;TrustServerCertificate=True;"
},
"VnPay": {
"TmnCode": "YOUR_TMN_CODE",
"HashSecret": "YOUR_HASH_SECRET",
"BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
"ReturnUrl": "https://localhost:PORT/Payment/PaymentCallback"
}
}
Cập nhật Database (Migration):

Sử dụng Package Manager Console trong Visual Studio:

PowerShell
Update-Database
Hoặc dùng .NET CLI:

Bash
dotnet ef database update
Chạy ứng dụng:

Nhấn F5 trên Visual Studio hoặc gõ lệnh:

Bash
dotnet run
Truy cập trình duyệt theo địa chỉ https://localhost:<PORT> được hiển thị trên console.

👤 Tác giả
Họ và tên: Nguyễn Văn Vui

Email: nguyenvanvui2005bg@gmail.com

GitHub:https://github.com/Vui-Nguyen
