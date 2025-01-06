using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Linq;

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

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var user = db.ApplicationUsers.Where(u=>u.Id == _userManager.GetUserId(User)).FirstOrDefault();
            var conv = db.Conversations.Include(c=>c.Messages).Include(c=>c.Users).ThenInclude(u=>u.Profile).Where(c => c.Users.Contains(user));
            ViewBag.Conversations = conv;
            ViewBag.Count = conv.Count();
            ViewBag.UserId = _userManager.GetUserId(User);
            return View();
        }

        public IActionResult Index(int? id)
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var conversation = db.Conversations.Where(c => c.Id == id).FirstOrDefault();
            if (conversation == null)
            {
                TempData["message"] = "The conversation does not exist." + id;
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");
            }
            var conversationUsers = db.Users.Include("Profile").Where(u => u.Conversations.Contains(conversation));
            var user = _userManager.GetUserId(User);
            var applicationUser = db.ApplicationUsers.Where(u => u.Id == user).First();

            if (conversationUsers.Contains(applicationUser) == false)
            {
                TempData["message"] = "You can not view this conversation.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Following", "Follows");
            }

            ViewBag.Messages = db.Messages.Include("User").Include("User.Profile").Where(m => m.ConversationId == id);
            foreach(var u in conversationUsers)
            {
                if(u.Id == user)
                {
                    ViewBag.CurrentUser = u;
                }
                else
                {
                    ViewBag.OtherUser = u;
                }
            }
            ViewBag.CurrentConversation = id;
           
            return View();
        }

        public IActionResult Conversation(string? id)
        {
            var user = _userManager.GetUserId(User);
            var follows = db.Follows.Where(f => f.UserId == id && f.FollowerId == user && f.Status==true);
            var following = db.Follows.Where(f => f.UserId == user && f.FollowerId == id && f.Status == true);
            var profile = db.Profiles.Where(p => p.UserId == user).First();
            if (follows.Count() == 0 && following.Count()==0 && profile.IsPrivate==true)
            {
                TempData["message"] = "You can only talk to people you are friends with.";
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


            }
            //int? conversationId = db.Conversations
            //                    .Where(c => c.Users.Count() == 2 &&
            //                            c.Users.Any(u => u.Id == user) &&
            //                            c.Users.Any(u => u.Id == id)).Select(c=>(int?)c.Id).FirstOrDefault();

            int? Id = conversation.Id;

            return RedirectToAction("Index", new { Id } );
        }
    }
}
