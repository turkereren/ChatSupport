using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSupport.Models
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        [Key, MaxLength(32)]
        public string ChatId { get; set; } = default!;

        [MaxLength(100)]
        public string UserName { get; set; } = default!;

        public bool ClaimedBySupport { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

