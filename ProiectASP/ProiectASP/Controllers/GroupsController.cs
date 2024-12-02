using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Security.Cryptography.X509Certificates;


namespace ProiectASP.Controllers
{
    public class GroupsController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public IActionResult Index()
        {
            return View();
        }
        // View - ul de baza care primeste un userId si afiseaza toate grupurile acestui User
        public IActionResult Home(string? id)
        {
            var groups = db.UserGroups.Include("Group")
                                .Where(ug => ug.UserId == id);

            ViewBag.Number = groups.Count();
            ViewBag.Groups = groups;
            return View();
        }
    }
}
