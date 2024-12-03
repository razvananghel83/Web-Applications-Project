using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Security.Cryptography.X509Certificates;


namespace ProiectASP.Controllers
{
    public class GroupsController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
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

        public IActionResult New()
        {
            Group group = new Group();
            return View( group );
        }

        [HttpPost]
        public async Task<IActionResult> New(Group group, IFormFile? Image)
        {
            // adaug imaginea in folder si in tabel
            group.Image = "/images/" + "default_group_pic.png"; //img default

            if (Image != null && Image.Length>0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if(!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "The file needs to be a jpg, jpeg or png.");
                    return View(group);
                }

                var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;

                using(var fileStream  = new FileStream(storagePath,FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(group.Image));
                group.Image = databaseFileName;
            }
            // daca nu am bagat imagine, ia pe aia default

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                group.ModeratorId = userId;

                db.Groups.Add(group);
                db.SaveChanges();

                UserGroup usergroup = new UserGroup();
                usergroup.UserId = userId;
                usergroup.GroupId = group.Id;
                db.UserGroups.Add(usergroup);
                db.SaveChanges();

                TempData["message"] = "Articolul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");

            }
            else
            {
                return View(group);
            }

        }
        
        

        [Authorize]
        // View - ul de baza care primeste un userId si afiseaza toate grupurile acestui User
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var groups = db.UserGroups.Include("Group").Include("User")
                                .Where(ug => ug.UserId == userId);

            ViewBag.Number = groups.Count();
            ViewBag.Groups = groups;
            return View();
        }
    
    
    
        public IActionResult Show(int? id)
        {
            Group group = db.Groups.
                            Where(g => g.Id == id)
                            .First();
            var Users = db.UserGroups.Include("User").Where(ug => ug.GroupId == id);
            ViewBag.Users = Users;
           return View(group);
        }

        public IActionResult All()
        {
            var userId = _userManager.GetUserId(User);
            var groups = db.Groups
                            .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)));


            ViewBag.Groups = groups;
            return View();
        }

        public IActionResult YourGroups()
        {
            var userId = _userManager.GetUserId(User);
            var groups = db.Groups.Where(g => g.ModeratorId == userId);

            ViewBag.Groups = groups;
            ViewBag.Count = groups.Count();
            return View();
        }

        public IActionResult Delete(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();

            db.Groups.Remove(group);
            db.SaveChanges();

            return RedirectToAction("YourGroups");
        }

        public IActionResult Edit(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();
            return View(group);
        }


        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            Group group = db.Groups.Find(id);

            group.Name = requestGroup.Name;
            group.Description = requestGroup.Description;
            db.SaveChanges();
            return RedirectToAction("YourGroups");
        }

    }
}
