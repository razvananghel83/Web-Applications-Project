using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        
        public string Username { get; set; }
        public string Description { get; set; }

        public bool IsPrivate { get; set; }


        //one(profil) to one(user)

        public string? UserId {  get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
