namespace ProiectASP.Models
{
    public class UserGroup
    {
        public string? UserId { get; set; }

        public int? GroupId { get; set; }

        public bool? IsSubscribed { get; set; }
        public bool? Status { get; set; } // true daca apartine grupului, false daca nu

        public virtual ApplicationUser? User { get; set; }  
        public virtual Group? Group { get; set; }
    }
}
