using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using ProiectASP.Data;
using ProiectASP.Models;

namespace ProiectASP.Controllers
{
    [Authorize]
    public class FollowsController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public FollowsController(
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


        // vedem prietenii din aplicatie 
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var userId = _userManager.GetUserId(User);

            var search = "";
            IEnumerable<ApplicationUser> friends;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<string> UserIDs = db.ApplicationUsers.Where
                                    (
                                        u => u.UserName.Contains(search) &&
                                        u.UserName.Contains(search)
                                    )
                                    .Select(u => (string)u.Id).ToList();
                List<string> FriendIDS = UserIDs
                            .Where(u => Friends(userId, u))
                            .ToList();
                friends = db.ApplicationUsers.Where
                            (
                                u => FriendIDS.Contains((string)u.Id)
                            );

            }
            else
            {
                List<string> UserIDs = db.ApplicationUsers
                                    .Select(u => (string)u.Id).ToList();
                List<string> FriendIDS = UserIDs
                            .Where(u => Friends(userId, u))
                            .ToList();
                friends = db.ApplicationUsers.Where
                            (
                                u => FriendIDS.Contains((string)u.Id)
                            );


            }
            ViewBag.Friends = friends;
            ViewBag.Search = search;

            ViewBag.Number = friends.Count();
            
            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Follows/Index/?search=" + search;
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Follows/Index";
            }

            return View();
        }


        // vedem prietenii din aplicatie 
        public IActionResult Followers(string? id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var userId = id;

            var search = "";
            IEnumerable<ApplicationUser> followers;
            List<string?> FollowersList = db.Follows.Where
                                    (
                                       u => u.UserId == userId && u.Status == true
                                    )
                                    .Select(u => u.FollowerId).ToList();
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<string> SearchedFollowers = db.ApplicationUsers.Where
                                    (
                                       u => FollowersList.Contains((string)u.Id) &&
                                        u.UserName.Contains(search)
                                    ).Select(u => u.Id).ToList();

                followers = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => SearchedFollowers.Contains((string)u.Id)
                            );

            }
            else
            {

                followers = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => FollowersList.Contains((string)u.Id)
                            );


            }
            ViewBag.followers = followers;
            ViewBag.Search = search;

            ViewBag.Number = followers.Count();
            ViewBag.EsteAdmin = User.IsInRole("Admin");

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Follows/Followers/?search=" + search;
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Follows/Followers";
            }

            return View();
        }

        public IActionResult Following(string? id)
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            var userId = id;

            var search = "";
            IEnumerable<ApplicationUser> following;
            List<string?> FollowingList = db.Follows.Where
                                    (
                                       u => u.FollowerId == userId && u.Status == true
                                    )
                                    .Select(u => u.UserId).ToList();
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();



                List<string> SearchedFollowers = db.ApplicationUsers.Where
                                    (
                                       u => FollowingList.Contains((string)u.Id) &&
                                        u.UserName.Contains(search)
                                    ).Select(u => u.Id).ToList();

                following = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => SearchedFollowers.Contains((string)u.Id)
                            );

            }
            else
            {

                following = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => FollowingList.Contains((string)u.Id)
                            );


            }
            ViewBag.following = following;
            ViewBag.Search = search;

            ViewBag.Number = following.Count();
            ViewBag.EsteAdmin = User.IsInRole("Admin");

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Follows/Following/?search=" + search;
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Follows/Following";
            }

            return View();
        }

        public IActionResult AllUsers()
        {


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var userId = _userManager.GetUserId(User);

            var search = "";
            IEnumerable<ApplicationUser> users;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();



                List<string> UserIDs = db.ApplicationUsers.Where
                                    (
                                        u => u.UserName.Contains(search) && u.Id != userId 
                                    )
                                    .Select(u => (string)u.Id).ToList();


                users = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => UserIDs.Contains((string)u.Id)
                            );

            }
            else
            {
                List<string> UserIDs = db.ApplicationUsers
                                    .Select(u => (string)u.Id).ToList();
                users = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => UserIDs.Contains((string)u.Id) && u.Id != userId
                            );


            }


            ViewBag.Users = users;
            ViewBag.Search = search;
            ViewBag.CurrentUser = userId;
            ViewBag.Follows = db.Follows.Where(f=>f.FollowerId==userId && f.Status==true).Select(f => f.UserId).ToList();
            ViewBag.Sent = db.Follows.Where(f => f.FollowerId == userId && f.Status == false).Select(f => f.UserId).ToList();
            ViewBag.Number = users.Count();
            ViewBag.EsteAdmin = User.IsInRole("Admin");

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Follows/AllUsers/?search=" + search;
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Follows/AllUsers";
            }
            return View();
        }


        public IActionResult Follow(string? id)
        {
            var userId = _userManager.GetUserId(User);


            Follow followRequest = new Follow();
            followRequest.FollowerId = userId;
            followRequest.UserId = id;
            followRequest.Status = false;


            db.Follows.Add(followRequest);
            db.SaveChanges();

            TempData["message"] = "Follow request has been sent.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("AllUsers");
        }


        public IActionResult UserRequests()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            var userId = _userManager.GetUserId(User);

            var search = "";
            IEnumerable<ApplicationUser> myRequests;
            List<string?> Requests = db.Follows.Where
                                    (
                                       u => u.UserId == userId && u.Status == false
                                    )
                                    .Select(u => u.FollowerId).ToList();
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();



                List<string> SearchedRequests = db.ApplicationUsers.Where
                                    (
                                       u => Requests.Contains((string)u.Id) &&
                                        u.UserName.Contains(search)
                                    ).Select(u => u.Id).ToList();

                myRequests = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => SearchedRequests.Contains((string)u.Id)
                            );

            }
            else
            {

                myRequests = db.ApplicationUsers.Include("Profile").Where
                            (
                                u => Requests.Contains((string)u.Id)
                            );


            }
            ViewBag.Requests = myRequests;
            ViewBag.Search = search;

            ViewBag.Number = myRequests.Count();

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Follows/Following/?search=" + search;
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Follows/Following";
            }

            return View();
        }


        public IActionResult Accept(string? id)
        {
            var UserId = _userManager.GetUserId(User);

            var requests = db.Follows.Where(
                            f => f.UserId == UserId && f.FollowerId == id
                            );
            if(requests.Count() < 1)
            {
                TempData["message"] = "You can not accept this request.";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("UserRequests");
            }

            Follow Request = requests.First();
            Request.Status = true;
            db.SaveChanges();

            TempData["message"] = "The request has been accepted.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("UserRequests");
        }

        public IActionResult Reject(string? id)
        {
            var UserId = _userManager.GetUserId(User);

            var requests = db.Follows.Where(
                            f => f.UserId == UserId && f.FollowerId == id
                            );
            if (requests.Count() < 1)
            {
                TempData["message"] = "You can not reject this request.";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("UserRequests");
            }

            Follow Request = requests.First();
            db.Remove(Request);
            db.SaveChanges();

            TempData["message"] = "The request has been rejected.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("UserRequests");
        }



        public bool Friends(string? user1Id, string? user2Id)
        {

            var relatii = db.Follows
                .Where(f =>
                (f.UserId == user1Id && f.FollowerId == user2Id && f.Status == true) ||
                (f.FollowerId == user1Id && f.UserId == user2Id && f.Status == true));
            if (relatii.Count() == 2)
            {
                return true;
            }
            return false;
        }
    }
}
