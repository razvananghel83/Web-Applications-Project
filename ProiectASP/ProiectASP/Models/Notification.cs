using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime EventDate { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int? PostId { get; set; }
    }

}
