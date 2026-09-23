namespace DATN.Models.DTOs
{
    public class ChatMessageDto
    {
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}