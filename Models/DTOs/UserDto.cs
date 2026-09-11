namespace DATN.Models.DTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? RoleName { get; set; }

        public bool IsLocked { get; set; }
    }
}