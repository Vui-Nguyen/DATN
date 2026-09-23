namespace DATN.Models.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerAvatar { get; set; }
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}