using ProiectASP.Data;
using ProiectASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ArticlesApp.Controllers
{
    [Authorize]
    public class LikesController : Controller
    {
        // PASUL 10: useri si roluri 

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public LikesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // Adaugarea unui comentariu asociat unui articol in baza de date

        public IActionResult Show(int? id)
        {
            //var UserList = db.Likes.Where(l => l.PostId == id).Select(l => l.UserId).ToList();

            //var Users = db.ApplicationUsers.Include("Profile").Where(u => UserList.Contains(u.Id));
            //ViewBag.Users=Users;
            //return View();


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            List<string> UserList = db.Likes.Where(l => l.PostId == id).Select(l => l.UserId).ToList();


            var search = "";
            IEnumerable<ApplicationUser> users;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();


                List<string> userIDs = db.ApplicationUsers.Where
                                    (
                                        u => u.UserName.Contains(search) && UserList.Contains(u.Id)
                                    )
                                    .Select(u => u.Id).ToList();
                users = db.ApplicationUsers.Include("Profile").Where(u => userIDs.Contains(u.Id));
            }
            else
            {
                 users = db.ApplicationUsers.Include("Profile").Where(u => UserList.Contains(u.Id));

            }
            ViewBag.Users = users;
            ViewBag.Search = search;
            ViewBag.Number = users.Count();
            ViewBag.PostId = id; 

            int perPage = 6;
            int totalItems = users.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedUsers = users.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Users = paginatedUsers;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/All/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Groups/All/?page";
            }
            return View();

        }
        [HttpPost]
        public IActionResult New(Like lik)
        {
            // Obține ID-ul utilizatorului curent
            var userId = _userManager.GetUserId(User);

            // Verifică dacă utilizatorul a dat deja like pentru acest post
            var existingLike = db.Likes.Where(l => l.UserId == userId && l.PostId==lik.PostId);

            if (existingLike.Count() ==0)
            {
                // Dacă nu există un like anterior, setează UserId-ul și salvează noul like
                lik.UserId = userId;

                if (ModelState.IsValid)
                {
                    db.Likes.Add(lik);
                    db.SaveChanges();
                    return Redirect("/Posts/Show/" + lik.PostId);
                }
            }


            // Redirecționează înapoi la post, indiferent dacă există sau nu un like
            return Redirect("/Posts/Show/" + lik.PostId);
        }






        // Stergerea unui comentariu asociat unui articol din baza de date
        // Se poate sterge comentariul doar de catre userii cu rolul de Admin 
        // sau de catre utilizatorii cu rolul de User sau Editor, doar daca 
        // acel comentariu a fost postat de catre acestia

        //[HttpPost]
        //[Authorize(Roles = "User,Admin")]
        //public IActionResult Delete(int id)
        //{
        //    var lik = db.Likes.FirstOrDefault(l => l.PostId == id);

        //    if (lik.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
        //    {
        //        db.Likes.Remove(lik);
        //        db.SaveChanges();
        //        return Redirect("/Posts/Show/" + lik.PostId);
        //    }
        //    else
        //    {
        //        TempData["message"] = "Nu aveti dreptul sa stergeti like-ul";
        //        TempData["messageType"] = "alert-danger";
        //        return RedirectToAction("Index", "Posts");
        //    }
        //}
        [HttpPost]
        public IActionResult Delete(string userId, int postId)
        {
            // Găsește like-ul bazat pe UserId și PostId
            var lik = db.Likes.FirstOrDefault(l => l.UserId == userId && l.PostId == postId);

            if (lik != null)
            {
                if (lik.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    db.Likes.Remove(lik);
                    db.SaveChanges();
                    return Redirect("/Posts/Show/" + postId);
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să ștergeți like-ul";
                    TempData["messageType"] = "alert-danger";
                }
            }
            else
            {
                TempData["message"] = "Like-ul nu a fost găsit";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index", "Posts");
        }


    }



}
