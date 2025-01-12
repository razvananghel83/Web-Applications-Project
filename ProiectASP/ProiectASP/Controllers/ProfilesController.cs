using Microsoft.AspNetCore.Mvc;
using ProiectASP.Data;
using ProiectASP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Principal;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Authorization;

namespace ProiectASP.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(
        IWebHostEnvironment env, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Details(string userId)
        {
            var profile = await db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound();

            }

            return View(profile);
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Profile profile, IFormFile profilePicture, string? id)
        {


            ModelState.Clear();
            profile.ProfileImage = "/images/default-profile-pic.jpg";
            if(profile.DateOfBirth == null)
            {
                ModelState.AddModelError("Birth", "The date of birth can't be null");
                return View(profile);
            }
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                    var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfileImage", "The file needs to be a jpg, jpeg  ,png ");
                        return View(profile);
                    }

                    var filePath = Path.Combine(_env.WebRootPath, "images", profilePicture.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }

                    ModelState.Remove(nameof(profile.ProfileImage));
                    profile.ProfileImage = $"/images/{profilePicture.FileName}";
                }

                profile.UserId = id;
                var user = db.ApplicationUsers.Where(u => u.Id ==id ).FirstOrDefault();
                profile.Username = user.UserName;
                db.Add(profile);
                //await db.SaveChangesAsync();
                db.SaveChanges();
                //return RedirectToAction(nameof(Details), new { userId = profile.UserId });
                var  returnUrl = Url.Content("~/");
                var thisUser = db.Users.Where(u => u.Id == id).FirstOrDefault();
                //var url = Url.Page("/Identity/Account/RegisterConfirmation", new { email = thisUser.Email, returnUrl = returnUrl });
                 string url = "https://localhost:7281/Identity/Account/RegisterConfirmation?email=" + thisUser.Email + "&returnUrl=" + returnUrl;
                return Redirect(url);
        }


        public IActionResult Show(string? id)
        {
            var user = db.ApplicationUsers.Where(u => u.Id == id).FirstOrDefault();
            var profile = db.Profiles.Where(p => p.UserId == id).FirstOrDefault();
            var userId = _userManager.GetUserId(User);
            bool canSeePosts = false;
            bool canEdit = false;
            bool conv = false;

            if (User.IsInRole("Admin"))
            {
                canSeePosts = true;
                canEdit = true;
                conv= true;
            }
            if (profile.IsPrivate == false)
            {
                conv = true;
                canSeePosts = true;
            }
            else
            {
                var followers = db.Follows.Where(f => f.UserId == user.Id && f.Status==true).Select(f => f.FollowerId).ToList(); // user.Id's followers
                var following = db.Follows.Where(f => f.FollowerId == user.Id && f.Status == true).Select(f => f.UserId).ToList(); // user.Id's following

                if (followers.Contains(userId))
                {
                    canSeePosts = true;
                }
                if (followers.Contains(userId) && following.Contains(userId))
                {
                    conv = true;
                }
            }
            if (userId == id)
            {
                canEdit = true;
                canSeePosts = true;
            }


            ViewBag.canSeePosts = canSeePosts;
            ViewBag.canEdit = canEdit;
            ViewBag.conv = conv;
            
            ViewBag.UserId = userId;


            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.Posts = db.Posts.Where(p => p.UserId == id);
            ViewBag.User = user;
            ViewBag.Profile = profile;

            ViewBag.NrPosts = db.Posts.Where(p => p.UserId == id).Count();
            ViewBag.NrFollowers = db.Follows.Where(f => f.UserId == id && f.Status==true).Count();
            ViewBag.NrFollowing = db.Follows.Where(f=> f.FollowerId == id && f.Status == true).Count();
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            Console.WriteLine($"Edit method called with ptofile id: {id}");

            Profile profile = db.Profiles.Where(pro => pro.Id == id)
                                        .First();

            if (profile == null)
            {
                Console.WriteLine($"No profile found with id: {id}");
                return NotFound();
            }

            if (profile.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            return View(profile);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? newDescription, DateTime? newDob, IFormFile? newProfileImage, bool? newIsPrivate, string? newUsername)
        {
            var profile = db.Profiles.Find(id);

            if (profile == null)
            {
                return NotFound();
            }

            if (profile.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            ModelState.Remove(nameof(Profile.ProfileImage));

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(newDescription))
                {
                    profile.Description = newDescription;
                }

                if (newDob.HasValue)
                {
                    profile.DateOfBirth = newDob.Value;
                }

                if (newIsPrivate.HasValue)
                {
                    profile.IsPrivate = newIsPrivate.Value;
                }

                if (newProfileImage != null && newProfileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(newProfileImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfileImage", "The file needs to be a jpg, jpeg, or png.");
                        return View(profile);
                    }

                    var storagePath = Path.Combine(_env.WebRootPath, "images", Path.GetFileName(newProfileImage.FileName));
                    var databaseFileName = "/images/" + Path.GetFileName(newProfileImage.FileName);

                    try
                    {
                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await newProfileImage.CopyToAsync(fileStream);
                        }
                        profile.ProfileImage = databaseFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfileImage", "Error uploading the file: " + ex.Message);
                        return View(profile);
                    }
                }

                var User = db.ApplicationUsers.Where(u => u.Id == profile.UserId).FirstOrDefault();
                profile.Username = newUsername;
                User.UserName = newUsername;
                User.NormalizedUserName = User.UserName.ToUpper();
                db.SaveChanges();

                TempData["message"] = "Profile modified successfully!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = profile.UserId });
            }

            return View(profile);
        }



        // Other actions for Delete, etc.

        //public async Task<IActionResult> Delete( Profile profile )
        //{
        //    await db.Remove(profile);
        //    return View();
        //}


        public async Task<IActionResult> Search(string query)
        {
            var profiles = await db.Profiles
                .Where(p => p.User.UserName.Contains(query) && !p.IsPrivate)
                .ToListAsync();
            return View(profiles);
        }


    }
}
