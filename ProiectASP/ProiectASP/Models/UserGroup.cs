namespace ProiectASP.Models
{
    public class UserGroup
    {
        public string? UserId { get; set; }

        public int? GroupId { get; set; }

        public bool? IsSubscribed { get; set; }

        public virtual ApplicationUser? User { get; set; }  
        public virtual Group? Group { get; set; }
    }
}
