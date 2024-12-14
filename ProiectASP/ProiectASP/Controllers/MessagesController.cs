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

        public IActionResult Add([FromForm] Message mesaj)
        {
            mesaj.Date= DateTime.Now;
            db.Messages.Add( mesaj );
            db.SaveChanges();

            return RedirectToAction("Index", "Conversations", new {id = mesaj.ConversationId});
        }

    }
}
