using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
    
        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }
        
        public int ModeratorId { get; set; }

        // many (postari) - one (grup)

        public virtual ICollection<Post>? Posts { get; set; }

        //many(users) - many(groups)

        public virtual ICollection<UserGroup>? UserGroups { get; set; }


    }
}
