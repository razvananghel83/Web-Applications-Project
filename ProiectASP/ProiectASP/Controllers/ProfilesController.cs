using Microsoft.AspNetCore.Mvc;
using ProiectASP.Data;
using ProiectASP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Linq;

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

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Profile profile, IFormFile profilePicture)
        {
            //if (ModelState.IsValid)
            //{
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    var filePath = Path.Combine(_env.WebRootPath, "images", profilePicture.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }
                    profile.ProfileImage = $"/images/{profilePicture.FileName}";
                }

                profile.UserId = _userManager.GetUserId(User);
                db.Add(profile);
                //await db.SaveChangesAsync();
                db.SaveChanges();
                return RedirectToAction(nameof(Details), new { userId = profile.UserId });
            //}
            return View(profile);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {

            Profile profile = db.Profiles.Where(pro => pro.Id == id)
                                    .First();

            if (profile == null)
            {
                return NotFound();
            }


            if (profile.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            return View(profile);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? newDescription, DateTime? newDob, IFormFile? newProfileImage, bool? newIsPrivate)
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

                    //var storagePath = Path.Combine(_env.WebRootPath, "images", newProfileImage.FileName);
                    var storagePath = Path.GetDirectoryName(newProfileImage.FileName);
                    var databaseFileName = "/images/" + newProfileImage.FileName;

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

                db.SaveChanges();

                TempData["message"] = "Profilul a fost modificat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Details", new { id });
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
