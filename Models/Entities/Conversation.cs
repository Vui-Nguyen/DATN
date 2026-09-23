using DATN.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models.Entities
{
    [Table("Conversations")]
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        public int CustomerId { get; set; }


        [Required]
        public int ShopId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(CustomerId))]
        public virtual User Customer { get; set; } = null!;

        [ForeignKey(nameof(ShopId))]
        public virtual Shop Shop { get; set; } = null!;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}