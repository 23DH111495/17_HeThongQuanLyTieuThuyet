using System;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Models;
using WebNovel.Data;
using System.Web;
using System.IO;

namespace WebNovel.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DarkNovelDbContext _context = new DarkNovelDbContext();

        // ✅ GET: MyProfile
        [HttpGet]
        public ActionResult MyProfile()
        {
            //if (Session["UserId"] == null)
            //    return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // ✅ POST: Update Info (giống cách xử lý ChangePassword)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateInfo(int id, string Username, string Email, string FirstName, string LastName)
        {
            //if (Session["UserId"] == null)
            //    return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            Console.WriteLine(user);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("MyProfile");
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                TempData["Error"] = "Username and Email cannot be empty!";
                return RedirectToAction("MyProfile");
            }

            user.Username = Username.Trim();
            user.Email = Email.Trim();
            user.FirstName = FirstName?.Trim();
            user.LastName = LastName?.Trim();
            user.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction("MyProfile");
        }

        // ✅ POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int id, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return HttpNotFound();

            if (user.PasswordHash != CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("MyProfile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("MyProfile");
            }

            user.PasswordHash = NewPassword;
            user.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("MyProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(int id, HttpPostedFileBase AvatarFile)
        {
            if (AvatarFile == null || AvatarFile.ContentLength == 0)
            {
                TempData["Error"] = "Please select an image to upload.";
                return RedirectToAction("MyProfile");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("MyProfile");
            }

            try
            {
                // Lưu ảnh vào database
                using (var reader = new BinaryReader(AvatarFile.InputStream))
                {
                    user.ProfilePictureData = reader.ReadBytes(AvatarFile.ContentLength);
                }
                user.ProfilePictureContentType = AvatarFile.ContentType;
                user.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                // ✅ Cập nhật session ngay sau khi lưu
                Session["ProfilePictureData"] = user.ProfilePictureData;
                Session["ProfilePictureContentType"] = user.ProfilePictureContentType;

                TempData["Success"] = "Avatar updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error uploading avatar: " + ex.Message;
            }

            return RedirectToAction("MyProfile");
        }






    }
}
