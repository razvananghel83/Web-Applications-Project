using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Like
    {

        [Key]
        public int Id { get; set; }
        
        // many(likes) - one(user)

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        //many(likes) - one(post)

        public int? PostId { get; set; }
        public virtual Post? Post { get; set; }
    }
}
