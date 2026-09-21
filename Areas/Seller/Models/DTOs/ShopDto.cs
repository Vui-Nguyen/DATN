public class ShopResponseDto
{
    public int ShopID { get; set; }
    public int UserID { get; set; }
    public string ShopName { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsLocked { get; set; }
    public string AvatarShop { get; set; }
}

