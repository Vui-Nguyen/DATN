namespace DATN.Models.DTOs
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    // 2. Dùng cho các tác vụ cần lấy dữ liệu ra (Get chi tiết, Get danh sách, Get Dashboard...)
    // Kế thừa từ ServiceResult để có sẵn Success và Message
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }
    }

}