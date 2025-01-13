using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace ProiectASP.Models
{
    public class ApplicationUser : IdentityUser
    {

        //many(posts ) - one(user )
        public virtual ICollection<Post>  Posts { get; set; }
        
        //many(comments) - one(user)
        public virtual ICollection<Comment>? Comments { get; set; }

        //many(messages) - one(user)

        public virtual ICollection<Message>? Messages { get; set; }

        //one(profil) - one (user)
        public virtual Profile? Profile { get; set; }

        //many(conversation) - many(user)

        public virtual ICollection<Conversation>? Conversations { get; set; }

        // many(likes) - one(user)
        public virtual ICollection<Like>? Likes { get; set; }

        //many(users) - many(groups)

        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        //many(users) - many(users) prin follow
        // un user are mai multi urmaritori
        // un user poate urmari mai multe persoane

        public virtual ICollection<Follow>? Follows { get; set; }
        public virtual ICollection<Follow>? Followers { get; set; }

        // un user are mai multe notificari
        public virtual ICollection<Notification>? Notifications { get; set; }
    }
}
