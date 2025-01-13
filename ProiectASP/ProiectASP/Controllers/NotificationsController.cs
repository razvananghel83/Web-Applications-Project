using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProiectASP.Data;
using ProiectASP.Models;

namespace ProiectASP.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var notifications = db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.EventDate)
                .Select(n => new Notification
                {
                    Id = n.Id,
                    Content = n.Content,
                    EventDate = n.EventDate,
                    PostId = n.PostId,
                    UserId = n.UserId

                }).ToList();

            return View( notifications );
        }

    }

}
