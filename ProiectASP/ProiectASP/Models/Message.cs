using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Message
    {

        [Key]
        public int Id { get; set; }
        public string Content { get; set; }

        public DateTime Date { get; set; }

        //many(mesaje) - one(Conversation)
        public int? ConversationId { get; set; }

        public virtual Conversation? Conversation { get; set; }

        //many(mesahe) - one(user)

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        
    }
}
