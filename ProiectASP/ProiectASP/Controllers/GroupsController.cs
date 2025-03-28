﻿using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using ProiectASP.Data;
using ProiectASP.Models;
using System.Linq;
using System.Net.WebSockets;
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
                    ModelState.AddModelError("GroupImage", "The file needs to be a jpg, jpeg or png.");
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
                usergroup.Status = true;
                db.UserGroups.Add(usergroup);
                db.SaveChanges();

                TempData["message"] = "The group has been created.";
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

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //search engine

            var search = "";
            IEnumerable<UserGroup> groups;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> groupsID = db.Groups.Where
                                    (
                                        g => g.Name.Contains(search)
                                    )
                                    .Select(g => (int)g.Id).ToList();
                List<int> groupsDescriptionID = db.Groups.Where
                                    (
                                        g => g.Description.Contains(search) &&
                                        !(groupsID.Contains(g.Id))
                                    ).Select(g => (int)g.Id).ToList();


                var groups1 = db.UserGroups.Include("Group").Include("User").
                    Where(ug => groupsID.Contains((int)ug.GroupId)
                                    && ug.UserId == userId
                                        && ug.Status == true)
                                                        .OrderByDescending(ug => ug.GroupId);
                var groups2 = db.UserGroups.Include("Group").Include("User").
                    Where(ug => groupsDescriptionID.Contains((int)ug.GroupId)
                                    && ug.UserId == userId
                                       && ug.Status == true)
                                                        .OrderByDescending(ug => ug.GroupId);
                groups = groups1.Concat(groups2);

            }
            else
            {
                groups = db.UserGroups.Include("Group").Include("User")
                                   .Where(ug => ug.UserId == userId && ug.Status == true)
                                   .OrderByDescending(ug => ug.GroupId);

            }
            ViewBag.Groups = groups;
            ViewBag.Search = search;
            ViewBag.Number = groups.Count();

            int perPage = 6;
            int totalItems = groups.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if(!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = groups.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Groups/Index/?page";
            }

            return View();



        }
    
        public IActionResult Show(int? id)
        {
            Group group = db.Groups.
                            Where(g => g.Id == id)
                            .First();


            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var member = db.UserGroups.Where(ug => ug.GroupId == id && ug.UserId == _userManager.GetUserId(User) && ug.Status==true);
            var isSubscribed = db.UserGroups.Any(ug => ug.UserId == _userManager.GetUserId(User) 
                    && ug.GroupId == id && ug.IsSubscribed == true);
            ViewBag.IsSubscribed = isSubscribed;


            if (member.Count() == 0)
                ViewBag.Member = false;
            else
                ViewBag.Member = true;

            if (group.ModeratorId == _userManager.GetUserId(User) )
                ViewBag.Moderator = true;
            else
                ViewBag.Moderator = false;
            ViewBag.Admin = User.IsInRole("Admin");
            ViewBag.Posts = db.Posts.Include("User").Include("User.Profile").Where(p =>p.GroupId == id);
            return View(group);
        }

        public IActionResult Members(int? id)
        {

            var userId = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            IEnumerable<UserGroup> users;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<string> userIDs = db.Users.Where
                                    (
                                        u=>u.UserName.Contains(search)
                                    )
                                    .Select(u => u.Id).ToList();



                 users = db.UserGroups.Include("User").Include("User.Profile").
                    Where(ug => userIDs.Contains(ug.UserId))
                    .Where(ug => ug.GroupId == id && ug.Status == true);
            }
            else
            {
                users = db.UserGroups.Include("User").Include("User.Profile")
                        .Where(ug => ug.GroupId == id && ug.Status == true);

            }
            ViewBag.Users = users;
            ViewBag.GroupID = id;
            ViewBag.Search = search;
            ViewBag.Number = users.Count();

            int perPage = 6;
            int totalItems = users.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = users.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/All/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Groups/All/?page";
            }

            Group group = db.Groups.
                            Where(g => g.Id == id)
                            .First();
            if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                ViewBag.Moderator = true;
            else
                ViewBag.Moderator = false;
            ViewBag.ModeratorId = group.ModeratorId;
            ViewBag.GroupId = group.Id;
            ViewBag.CurrentId = _userManager.GetUserId(User);

            return View();



        }

        public IActionResult Leave(int? id)
        {
            string userId = _userManager.GetUserId(User);
            var grup = db.Groups.Where(g=> g.Id == id).FirstOrDefault();
            if (grup == null)
            {

                TempData["message"] = "The group does not exist.";
                TempData["messageType"] = "alert-danger";
            }
            else
            {
                if (grup.ModeratorId != userId)
                {
                    var rm = db.UserGroups.Where(ug => ug.GroupId == id && ug.UserId == userId).First();
                    if (rm != null)
                    {
                        db.Remove(rm);
                        db.SaveChanges();
                    }
                    TempData["message"] = "You have left the group.";
                    TempData["messageType"] = "alert-success";

                }
                else
                {

                    TempData["message"] = "The Moderator can't leave the group";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", new { id = id });
                }
            }
            return RedirectToAction("All");
        
        }

        public IActionResult All()
        {
            //var userId = _userManager.GetUserId(User);
            //var groups = db.Groups
            //                .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)));


            //ViewBag.Groups = groups;
            //return View();


            var userId = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            IEnumerable<Group> groups;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> groupsID = db.Groups.Where
                                    (
                                        g => g.Name.Contains(search)
                                    )
                                    .Select(g => (int)g.Id).ToList();
                List<int> groupsDescriptionID = db.Groups.Where
                                    (
                                        g => g.Description.Contains(search) &&
                                        !(groupsID.Contains(g.Id))
                                    ).Select(g => (int)g.Id).ToList();


                var groups1 = db.Groups.
                    Where(g => groupsID.Contains((int)g.Id) )
                    .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)))
                                                        .OrderByDescending(g => g.Id);
                var groups2 = db.Groups.
                    Where(g => groupsDescriptionID.Contains((int)g.Id))
                    .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)))
                                                        .OrderByDescending(g => g.Id);
                groups = groups1.Concat(groups2);

            }
            else
            {
                groups = db.Groups
                    .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)));

            }
            ViewBag.Groups = groups;
            ViewBag.Search = search;
            ViewBag.Number = groups.Count();

            int perPage = 6;
            int totalItems = groups.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = groups.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

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

        public IActionResult Join(int? id)
        {
            UserGroup usergroup = new UserGroup();
            usergroup.UserId = _userManager.GetUserId(User);
            usergroup.GroupId = id;
            usergroup.Status = false;
            usergroup.IsSubscribed = false;


            db.UserGroups.Add(usergroup);
            db.SaveChanges();

            TempData["message"] = "Join request has been sent.";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("All");
        }



        public IActionResult JoinRequests(int? id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var group = db.Groups.FirstOrDefault(g => g.Id == id);
            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "You can't see other groups join requests.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("YourGroups");
            }

            var search = "";
            IEnumerable<ApplicationUser> requests;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<string> requestsID = db.ApplicationUsers
                            .Where(u => u.UserName.Contains(search) 
                                && u.UserGroups.Any( ug => ug.GroupId == id && ug.Status==false))
                            .Select(u => u.Id).ToList();

                var requests1 = db.ApplicationUsers.Include("Profile").
                    Where(u => requestsID.Contains(u.Id) );
                requests = requests1;

            }
            else
            {
                requests = db.ApplicationUsers.Include("Profile")
                                .Where(u => u.UserGroups.Any(ug => ug.GroupId == id && ug.Status == false));

            }
            ViewBag.GroupId = id;
            ViewBag.Requests = requests;
            ViewBag.Search = search;
            ViewBag.Number = requests.Count();
            ViewBag.CurrentGroup = db.Groups.Where(g => g.Id == id).First();

            int perPage = 12;
            int totalItems = requests.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = requests.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/JoinRequests/?id/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Groups/JoinRequests/?id/?page";
            }



            return View();
        }

        public IActionResult Accept([FromForm] UserGroup requestUserGroup)
        {


            var ug = db.UserGroups.Where(ug => ug.GroupId == requestUserGroup.GroupId && ug.UserId == requestUserGroup.UserId).First();
            var group = db.Groups.Where(g => g.Id == requestUserGroup.GroupId).First();
            var user = db.Users.Where(u => u.Id == requestUserGroup.UserId).First();

            if (group.ModeratorId == _userManager.GetUserId(User))
            {
                ug.Status = true;
                db.SaveChanges();
                TempData["message"] = "The user " + user.UserName + " has been accepted.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("JoinRequests",new { id = group.Id });
            }
            else
            {
                TempData["message"] = "You can not accept this request.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("YourGroups");
            }
        }

        public IActionResult Reject([FromForm] UserGroup requestUserGroup)
        {


            var ug = db.UserGroups.Where(ug => ug.GroupId == requestUserGroup.GroupId && ug.UserId == requestUserGroup.UserId).First();
            var group = db.Groups.Where(g => g.Id == requestUserGroup.GroupId).First();
            var user = db.Users.Where(u => u.Id == requestUserGroup.UserId).First();

            if (group.ModeratorId == _userManager.GetUserId(User))
            {
                db.UserGroups.Remove(ug);
                db.SaveChanges();
                TempData["message"] = "The user " + user.UserName + " has been rejected.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("JoinRequests", new { id = group.Id });
            }
            else
            {
                TempData["message"] = "You can not reject this request.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("YourGroups");
            }
        }

        public IActionResult Remove([FromForm] UserGroup requestUserGroup)
        {


            var ug = db.UserGroups.Where(ug => ug.GroupId == requestUserGroup.GroupId && ug.UserId == requestUserGroup.UserId).First();
            var group = db.Groups.Where(g => g.Id == requestUserGroup.GroupId).First();
            var user = db.Users.Where(u => u.Id == requestUserGroup.UserId).First();

            if (ug.UserId != group.ModeratorId)
            {
                if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    db.UserGroups.Remove(ug);
                    db.SaveChanges();
                    TempData["message"] = "The user " + user.UserName + " has been removed.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Members", new { id = group.Id });
                }
                else
                {
                    TempData["message"] = "You can not remove this user. Only the moderator can do that.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Members", new { id = group.Id });
                }
            }
            else
            {
                TempData["message"] = "The moderator can't be removed.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Members", new { id = group.Id });
            }
        }



        public IActionResult YourGroups()
        {
            

            //search engine

            var userId = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            IEnumerable<Group> groups;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> groupsID = db.Groups.Where
                                    (
                                        g => g.Name.Contains(search)
                                    )
                                    .Select(g => (int)g.Id).ToList();
                List<int> groupsDescriptionID = db.Groups.Where
                                    (
                                        g => g.Description.Contains(search) &&
                                        !(groupsID.Contains(g.Id))
                                    ).Select(g => (int)g.Id).ToList();


                var groups1 = db.Groups.
                    Where(g => groupsID.Contains((int)g.Id) && g.ModeratorId == userId)
                                                        .OrderByDescending(g => g.Id);
                var groups2 = db.Groups.
                    Where(g => groupsDescriptionID.Contains((int)g.Id) && g.ModeratorId == userId)
                                                        .OrderByDescending(g => g.Id);
                groups = groups1.Concat(groups2);

            }
            else
            {
                groups = db.Groups.Where(g => g.ModeratorId == userId);

            }
            ViewBag.Groups = groups;
            ViewBag.Search = search;
            ViewBag.Number = groups.Count();

            int perPage = 6;
            int totalItems = groups.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = groups.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/YourGroups/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Groups/YourGroups/?page";
            }

            return View();


        }

        public IActionResult Delete(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();

            if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Groups.Remove(group);
                db.SaveChanges();

                TempData["message"] = "The group has been deleted.";
                TempData["messageType"] = "alert-success";
                
                return RedirectToAction("YourGroups");
            }
            else
            {
                TempData["message"] = "You can not delete this group. You are not the moderator.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("YourGroups");
            }

        }

        public IActionResult DeleteShow(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();

            if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var groupPosts = db.Posts.Where(p => p.GroupId == id);
                foreach(var post in groupPosts)
                {
                    db.Posts.Remove(post);
                }
                db.Groups.Remove(group);
                db.SaveChanges();

                TempData["message"] = "The group has been deleted.";
                TempData["messageType"] = "alert-success";
                var ID = id;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You can not delete this group. You are not the moderator.";
                TempData["messageType"] = "alert-danger";
                var ID = id;
                return RedirectToAction("Show", new { id = ID});
            }

        }


        public IActionResult Edit(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();

            if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(group);
            }
            else
            {

                TempData["message"] = "You can only edit your own groups.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("YourGroups");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Group requestGroup, IFormFile? Image)
        {



            Group group = db.Groups.Find(id);

            if (ModelState.IsValid)
            {
                if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    group.Name = requestGroup.Name;
                    group.Description = requestGroup.Description;

                    if (Image != null && Image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("GroupImage", "The file needs to be a jpg, jpeg or png.");
                            return View(group);
                        }

                        var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                        var databaseFileName = "/images/" + Image.FileName;

                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        ModelState.Remove(nameof(group.Image));
                        group.Image = databaseFileName;
                    }


                    TempData["message"] = "The group has been modified.";
                    TempData["messageType"] = "alert-success";

                    db.SaveChanges();
                    return RedirectToAction("YourGroups");
                }
                else
                {

                    TempData["message"] = "You can only edit your own groups.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("YourGroups");
                }
            }
            else
            {

                return View(requestGroup);
            }
        }


        public IActionResult EditShow(int? id)
        {
            Group group = db.Groups.Where(g => g.Id == id).First();

            if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(group);
            }
            else
            {

                TempData["message"] = "You can only edit your own groups.";
                TempData["messageType"] = "alert-danger";
                var ID = id;
                return RedirectToAction("Show", new { id = ID });
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditShow(int id, Group requestGroup, IFormFile? Image)
        {



            Group group = db.Groups.Find(id);

            if (ModelState.IsValid)
            {
                if (group.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    group.Name = requestGroup.Name;
                    group.Description = requestGroup.Description;

                    if (Image != null && Image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ArticleImage", "The file needs to be a jpg, jpeg or png.");
                            return View(group);
                        }

                        var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                        var databaseFileName = "/images/" + Image.FileName;

                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        ModelState.Remove(nameof(group.Image));
                        group.Image = databaseFileName;
                    }


                    TempData["message"] = "The group has been modified.";
                    TempData["messageType"] = "alert-success";

                    db.SaveChanges();
                    var ID = id;
                    return RedirectToAction("Show", new { id = ID });
                }
                else
                {

                    TempData["message"] = "You can only edit your own groups.";
                    TempData["messageType"] = "alert-danger";
                    var ID = id;
                    return RedirectToAction("Show", new { id = ID });
                }
            }
            else
            {

                return View(requestGroup);
            }
        }


        //public IActionResult Back()
        //{
        //    string refererUrl = Request.Headers["Referer"].ToString();

        //    return Redirect(refererUrl);
        //}


        public IActionResult AddPost(int? id)
        {
            var user = _userManager.GetUserId(User);

            var members = db.UserGroups.Where(ug => ug.GroupId == id).Select(ug => ug.UserId);
            if(!members.Contains(user))
            {

                TempData["message"] = "You can not post in this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            ViewBag.GroupId = id;
            Post post = new Post();


            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost([FromForm] Post post, IFormFile? Image)
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
                    ModelState.AddModelError("PostImage", "The file needs to be a jpg, jpeg  ,png ,.gif, .mp4, .mov or .mp3");
                    return RedirectToAction("AddPost", new { id = post.GroupId });
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

                // trimit notificari la userii care sunt abonati la grup pentru ca a fost adaugata o postare noua
                NotifySubscribers( post.Id );

                TempData["message"] = "The post has been created succesfully!";
                TempData["messageType"] = "alert-success";
                var ID = post.GroupId;
                return RedirectToAction("Show", new { id=ID });

            }
            else
            {
                return View(post);
            }


        }
        public IActionResult PendingRequests()
        {

            var userId = _userManager.GetUserId(User);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            IEnumerable<UserGroup> groups;
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> groupsID = db.Groups.Where
                                    (
                                        g => g.Name.Contains(search)
                                    )
                                    .Select(g => (int)g.Id).ToList();
                List<int> groupsDescriptionID = db.Groups.Where
                                    (
                                        g => g.Description.Contains(search) &&
                                        !(groupsID.Contains(g.Id))
                                    ).Select(g => (int)g.Id).ToList();


                //var groups1 = db.Groups.
                //    Where(g => groupsID.Contains((int)g.Id))
                //    .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)))
                //                                        .OrderByDescending(g => g.Id);
                //var groups2 = db.Groups.
                //    Where(g => groupsDescriptionID.Contains((int)g.Id))
                //    .Where(g => !(g.UserGroups.Any(ug => ug.UserId == userId)))
                //                                        .OrderByDescending(g => g.Id);
                //groups = groups1.Concat(groups2);

                var groups1 = db.UserGroups.Include("Group")
                            .Where(ug=>groupsID.Contains((int)ug.GroupId))
                            .Where(ug => ug.UserId == userId && ug.Status == false);

                var groups2 = db.UserGroups.Include("Group")
                            .Where(ug => groupsDescriptionID.Contains((int)ug.GroupId))
                            .Where(ug => ug.UserId == userId && ug.Status == false);
                groups = groups1.Concat(groups2);

            }
            else
            {
                groups = db.UserGroups.Include("Group")
                            .Where(ug => ug.UserId == userId && ug.Status == false);

            }
            ViewBag.Groups = groups;
            ViewBag.Search = search;
            ViewBag.Number = groups.Count();

            int perPage = 6;
            int totalItems = groups.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }
            var paginatedGroup = groups.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)perPage);

            ViewBag.Groups = paginatedGroup;

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
        public IActionResult Subscribe(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            var subscription = db.UserGroups.FirstOrDefault(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (subscription == null)
            {
                TempData["message"] = "You are not a member of this group!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            subscription.IsSubscribed = !subscription.IsSubscribed.GetValueOrDefault();
            db.SaveChanges();

            if (subscription.IsSubscribed.GetValueOrDefault())
            {
                TempData["message"] = "Subscribed successfully!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Unsubscribed successfully!";
                TempData["messageType"] = "alert-success";
            }

            return RedirectToAction("Show", new { id = groupId });
        }

        public void NotifySubscribers(int postId)
        {
            var post = db.Posts.Include(p => p.User).Include(p => p.Group).FirstOrDefault(p => p.Id == postId);
            if (post == null) return;

            var subscriptions = db.UserGroups
                                  .Where(s => s.GroupId == post.GroupId && s.IsSubscribed == true)
                                  .Select(s => s.User)
                                  .ToList();

            foreach (var user in subscriptions)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Content = $"{post.User.UserName} has posted in {post.Group.Name}.",
                    EventDate = DateTime.Now,
                    PostId = postId
                };
                db.Notifications.Add(notification);
            }
            db.SaveChanges();
        }


    }
}
