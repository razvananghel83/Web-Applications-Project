using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
    
        public string Content { get; set; }

        public DateTime Date { get; set; }

        //many(comments) - one(post)
        public int? PostId { get; set; }

        public virtual Post? Post { get; set; }
        
        //many(comments) - one(user)
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

    }
}
