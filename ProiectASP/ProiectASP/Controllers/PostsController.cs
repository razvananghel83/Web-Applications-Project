using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

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

        public IActionResult Index()
        {
            var posts = db.Posts.Include(p => p.User).Include(p => p.User.Profile).ToList();
            ViewBag.Posts = posts;
            
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
            ViewBag.GroupId = null;

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
        public async Task<IActionResult> New( Post post, IFormFile? Image )
        {
            post.Date = DateTime.Now;
            // adaug imaginea in folder si in tabel
            //post.Image = "/images/" + "default_group_pic.png"; //img default

            if (Image != null && Image.Length > 0)
            {

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".mp3" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostImage", "The file needs to be a jpg, jpeg  ,png ,.gif .mp4 .mov .mp3");
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

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Post post = db.Posts.Where(art => art.Id == id)
                                         .First();



            if ((post.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Edit(int id, Post requestpost, IFormFile? Image)
        {
            Post post = db.Posts.Find(id);

            if (ModelState.IsValid)
            {
                if ((post.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
                {

                    post.Content = requestpost.Content;
                    post.Date = DateTime.Now;
                    if (Image != null && Image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".mp3" };

                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ArticleImage", "The file needs to be a jpg, jpeg, png, gif, mp3, mov or mp3");
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

                    TempData["message"] = "Articolul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {

                return View(requestpost);
            }
        }
        public IActionResult Show(int id)
        {
            Post post = db.Posts.Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                                         .Include("Comments.User.Profile")
                              .Where(post => post.Id == id)
                              .First();

            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.UserId = _userManager.GetUserId(User);
            ViewBag.Profile = db.Profiles.Where(profile => profile.UserId == post.UserId).FirstOrDefault();
            return View(post);
        }

        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit


        // Adaugarea unui comentariu asociat unui articol in baza de date
        // Toate rolurile pot adauga comentarii in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza comentariul
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Posts/Show/" + comment.PostId);
            }
            else
            {
                Post post = db.Posts.Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                               .Where(post => post.Id == comment.PostId)
                               .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);


                ViewBag.EsteAdmin = User.IsInRole("Admin");
                ViewBag.UserId = _userManager.GetUserId(User);

                return View(post);
            }
        }






        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            // Article article = db.Articles.Find(id);

            Post post = db.Posts.Include("Comments")
                                         .Where(post => post.Id == id)
                                         .First();

            if ((post.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                TempData["message"] = "The post has been deleted.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You don't have the right to delete other posts.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }





    }


}




