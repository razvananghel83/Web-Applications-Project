using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        //many(conversation) - many(user)

        public virtual ICollection<ApplicationUser>? Users { get; set; }

        //many(mesaje) - one(Conversation)

       public virtual ICollection<Message>? Messages { get; set; }

    }
}
