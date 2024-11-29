using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }


        public string Content { get; set; }


        // many ( posts ) - one ( user )
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        //many(commments) - one(post)
        public virtual ICollection<Comment>? Comments { get; set; }

        //many(postari) - one(group)
        public int? GroupId { get; set; }

        public virtual Group? Group { get; set; }

        // many(likes) - one(post)

        public virtual ICollection<Like>? Likes { get; set; }




    }


}
