using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSupport.Models
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(32)]
        public string ChatId { get; set; } = default!;

        [MaxLength(100)]
        public string Sender { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

