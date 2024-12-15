using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Data;
using ProiectASP.Models;

namespace ProiectASP.Controllers
{
    [Authorize]
    public class ConversationsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ConversationsController(
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

        public IActionResult All()
        {
            var user = db.ApplicationUsers.Where(u=>u.Id == _userManager.GetUserId(User)).FirstOrDefault();
            var conv = db.Conversations.Include(c => c.Users).Where(c => c.Users.Contains(user));
            ViewBag.Conversations = conv;
            ViewBag.Count = conv.Count();

            return View();
        }

        public IActionResult Index(int? conversationId)
        {

            var conversation = db.Conversations.Include(c=>c.Users).Where(c => c.Id == conversationId).FirstOrDefault();
            if (conversation == null)
            {
                TempData["message"] = "The conversation does not exist." + conversationId; 
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");
            }
            var user = _userManager.GetUserId(User);
            var applicationUser = db.ApplicationUsers.Where(u => u.Id == user).First();

            if(conversation.Users.Contains(applicationUser) == false)
            {
                TempData["message"] = "You can not view this conversation.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");
            }

            ViewBag.Messages = db.Messages.Include("User").Where(m=>m.ConversationId == conversationId);
            ViewBag.CurrentUser = user;
            ViewBag.Users = conversation.Users;
            ViewBag.CurrentConversation = conversationId;

            return View();
        }

        public IActionResult Conversation(string? id)
        {
            var user = _userManager.GetUserId(User);
            var follows = db.Follows.Where(f => f.UserId == id && f.FollowerId == user);

            if (follows.Count() == 0)
            {
                TempData["message"] = "You can only talk to people you follow.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");
            }

            Conversation conversation = db.Conversations
                                .Where(c=>c.Users.Count()==2 &&
                                        c.Users.Any(u => u.Id == user) &&
                                        c.Users.Any(u => u.Id == id)).FirstOrDefault();
            if (conversation == null)
            {
                conversation = new Conversation();
                conversation.Users = new List<ApplicationUser>();
                conversation.Users.Add(db.ApplicationUsers.Where(u => u.Id == id).First());
                conversation.Users.Add(db.ApplicationUsers.Where(u => u.Id == user).First());


                db.Conversations.Add(conversation);
                db.SaveChanges();

                TempData["message"] = "You can only talk to people you follow." + conversation.Users.Count() ;
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");

            }
            var conversationId = db.Conversations
                                .Where(c => c.Users.Count() == 2 &&
                                        c.Users.Any(u => u.Id == user) &&
                                        c.Users.Any(u => u.Id == id)).Select(c=>c.Id);


            return RedirectToAction("Index", new { conversationId});
        }
    }
}
