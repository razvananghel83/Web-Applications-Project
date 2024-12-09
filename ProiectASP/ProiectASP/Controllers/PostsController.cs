using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Net.NetworkInformation;

namespace ProiectASP.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public PostsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            // var Posts = db.Posts.Include("User");
            //in http : post.user.UserName
            var Posts = db.Posts.Include("User");
            // ViewBag.OriceDenumireSugestiva
            ViewBag.Posts = Posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();

        }
        //public IActionResult New()
        //{

        //    Models.Post post = new Models.Post();

        //    return View(post);
        //}
        ////public IActionResult SHOW(Id) { return View(); }
        ////public IActionResult New() { return View(); }

        //[HttpPost]
        //public IActionResult New(Post post)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        db.Posts.Add(post);
        //        db.SaveChanges();
        //        TempData["message"] = "Articolul a fost adaugat";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        return View();
        //    }
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Post post = new Post();


            return View(post);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul Editor si Admin pot adauga articole in platforma
        //[HttpPost]
        //[Authorize(Roles = "User,Admin")]
        //public IActionResult New(Post post)
        //{


        //    // preluam Id-ul utilizatorului care posteaza articolul
        //    post.UserId = _userManager.GetUserId(User);

        //    if (ModelState.IsValid)
        //    {
        //        db.Posts.Add(post);
        //        db.SaveChanges();
        //        TempData["message"] = "Postarea a fost adaugat";
        //        TempData["messageType"] = "alert-success";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {

        //        return View(post);
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> New(Post post, IFormFile? Image)
        {
            post.Date = DateTime.Now;
            // adaug imaginea in folder si in tabel
            //post.Image = "/images/" + "default_group_pic.png"; //img default

            if (Image != null && Image.Length > 0)
            {

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostImage", "The file needs to be a jpg, jpeg  ,png ,.gif .mp4 .mov");
                    return View(post);
                }

                var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.Image));
                post.Image = databaseFileName;
            }
          
            // daca nu am bagat imagine, ia pe aia default!

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                post.UserId = userId;
                post.GroupId = null;
                db.Posts.Add(post);
                db.SaveChanges();



                TempData["message"] = "The post has been created.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");

            }
            else
            {
                return View(post);
            }

        }
    }


}




