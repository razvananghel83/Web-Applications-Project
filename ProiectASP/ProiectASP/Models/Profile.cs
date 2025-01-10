using System.ComponentModel.DataAnnotations;

namespace ProiectASP.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username {  get; set; }
        public string Description { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }


        public string? ProfileImage {  get; set; }

        public bool IsPrivate { get; set; }



        //one(profil) to one(user)

        public string? UserId {  get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
