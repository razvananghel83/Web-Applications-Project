using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The group name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "The description is required.")]
        public string Description { get; set; }

        public string? Image { get; set; }

        public string? ModeratorId { get; set; }

        // many (postari) - one (grup)

        public virtual ICollection<Post>? Posts { get; set; }

        //many(users) - many(groups)

        public virtual ICollection<UserGroup>? UserGroups { get; set; }


    }
}
