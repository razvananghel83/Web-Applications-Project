using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProiectASP.Data;
using ProiectASP.Models;

namespace ProiectASP.Controllers
{
    [Authorize]

    public class MessagesController : Controller
    {



        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessagesController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;

        }


        public IActionResult Delete(int? id)
        {
            var mesaj = db.Messages.Where(m => m.Id == id).FirstOrDefault();

            if(mesaj==null)
            {

                TempData["message"] = "This message doesn't exist.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("All", "Conversations");
            }

            if(mesaj.UserId != _userManager.GetUserId(User))
            {

                TempData["message"] = "Can't only delete your own messages.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("All", "Conversations");
            }
            int? Id = mesaj.ConversationId;
            db.Messages.Remove(mesaj);
            db.SaveChanges();

            return RedirectToAction("Index", "Conversations", new { Id });

        }
        public IActionResult Add([FromForm] Message mesaj)
        {
            mesaj.Date= DateTime.Now;

            var conversation = db.Conversations.Where(c => c.Id == mesaj.ConversationId).FirstOrDefault();
            var convUser = db.Users.Where(u => u.Conversations.Contains(conversation));
            var currentUser = db.Users.Where(u => u.Id == _userManager.GetUserId(User)).FirstOrDefault();
            if(!convUser.Contains(currentUser))
            {

                TempData["message"] = "Can't send messages to this conversation.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("All", "Conversations");
            }

            db.Messages.Add( mesaj );
            db.SaveChanges();

            int? id = mesaj.ConversationId;
             
            return RedirectToAction("Index", "Conversations", new { id });
        }

    }
}
