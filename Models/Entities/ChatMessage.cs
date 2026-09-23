using DATN.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int ChatMessageId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; } = null!;

        [ForeignKey(nameof(SenderId))]
        public virtual User Sender { get; set; } = null!;
    }
}