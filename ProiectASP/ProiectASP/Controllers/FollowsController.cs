using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Hosting;
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
                friends = db.ApplicationUsers.Include("Profile").Where
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
                friends = db.ApplicationUsers.Include("Profile").Where
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


        // vedem followerii din aplicatie 
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

            var prfls = db.Profiles.Select(p => p.UserId).ToList();
            var userswithoutprofile = db.ApplicationUsers.Where(u => !prfls.Contains(u.Id));
            foreach (var u in userswithoutprofile)
            {
                db.Remove(u);
                
            }
            db.SaveChanges();

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
            var following = db.Profiles.Where(p => p.UserId == id).FirstOrDefault();

            Follow followRequest = new Follow();
            followRequest.FollowerId = userId;
            followRequest.UserId = id;
            if (following.IsPrivate == false)
            {
                followRequest.Status = true;
                TempData["message"] = "User has been followed.";
                TempData["messageType"] = "alert-success";

            }
            else
            {
                TempData["message"] = "Follow request has been sent.";
                TempData["messageType"] = "alert-success";
                followRequest.Status = false;
            }

            db.Follows.Add(followRequest);
            db.SaveChanges();


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

        public IActionResult Delete(string? id)
        {
            if(!User.IsInRole("Admin"))
            {
                TempData["message"] = "You can not delete users.";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("AllUsers");
            }
            var user = db.Users.Where(u => u.Id == id).FirstOrDefault();
            var likes = db.Likes.Where(l => l.UserId == id);
            var profile = db.Profiles.Where( p=> p.UserId == id).FirstOrDefault();
            var comentarii = db.Comments.Where(c => c.UserId == id);
            var posts = db.Posts.Where(p=>p.UserId == id);
            var conversations = db.Conversations.Where(c => c.Users.Contains(user));
            var groups = db.Groups.Where(g => g.ModeratorId == id);
            foreach (var like in likes)
                db.Remove(like);
            foreach (var com in comentarii)
                db.Remove(com);
            foreach (var post in posts)
            {
                var postCom = db.Comments.Where(c => c.PostId == post.Id);

                foreach (var c in postCom)
                    db.Remove(c);

                var postLikes = db.Likes.Where(l => l.PostId == post.Id);
                foreach (var pl in postLikes)
                    db.Remove(pl);
                db.Remove(post);
            }
            foreach (var conversation in conversations)
            {
                var messages = db.Messages.Where(m => m.ConversationId == conversation.Id);
                foreach(var message in messages)
                     db.Remove(message); 
            }
            foreach (var group in groups)
            {
                var gposts = db.Posts.Where(p => p.GroupId == group.Id);
                foreach(var p in gposts)
                {
                    var postCom = db.Comments.Where(c => c.PostId == p.Id);

                    foreach (var c in postCom)
                        db.Remove(c);

                    var postLikes = db.Likes.Where(l => l.PostId == p.Id);
                    foreach (var pl in postLikes)
                        db.Remove(pl);

                    db.Remove(p);
                }
                db.Remove(group);
            }
            var follow = db.Follows.Where(f => f.FollowerId == id || f.UserId == id);
            foreach (var f in follow)
            {
                db.Remove(f);
            }

            db.Remove(profile);
            db.Remove(user);
            db.SaveChanges();
            return RedirectToAction("AllUsers");

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
    

        [HttpPost]
        public IActionResult Unfollow(string userId)
        {

            var currentUserId = _userManager.GetUserId(User);
            var followRequest = db.Follows.FirstOrDefault(f => f.FollowerId == currentUserId && f.UserId == userId);

            if (followRequest != null)
            {
                db.Follows.Remove(followRequest);
                db.SaveChanges();
                TempData["message"] = "You have successfully unfollowed the user.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Follow relationship not found.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("AllUsers");
        }

        public IActionResult RemoveFollower(string userId)

        {

            var currentUserId = _userManager.GetUserId(User);
            var followRequest = db.Follows.FirstOrDefault(f => f.FollowerId == userId && f.UserId == currentUserId);

            if (followRequest != null)
            {
                db.Follows.Remove(followRequest);
                db.SaveChanges();
                TempData["message"] = "You have successfully unfollowed the user.";
                TempData["messageType"] = "alert-success";
            }

            else
            {
                TempData["message"] = "Follow relationship not found.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("AllUsers");
        }

    }

}

