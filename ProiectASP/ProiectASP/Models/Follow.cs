using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Follow
    {

        public int Status { get; set; }

        //many(users) - many(users) prin follow
        // un user are mai multi urmaritori
        // un user poate urmari mai multe persoane

        public string? UserId { get; set; }
    
        public string? FollowerId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ApplicationUser? Follower {  get; set; } 

    }
}
