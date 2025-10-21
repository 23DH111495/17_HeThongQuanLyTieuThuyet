using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;
using AdminModel = WebNovel.Models.Admin;
using System.IO;

namespace WebNovel.Areas.Admin.Controllers
{
    public class User_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: Admin/User_Manager
        public ActionResult User_Manager(string search, string sortBy = "created", string sortDirection = "desc",
            string statusFilter = "all", string emailFilter = "all", string roleFilter = "all", int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.EmailFilter = emailFilter;
            ViewBag.RoleFilter = roleFilter;

            List<UserViewModel> users = GetFilteredUsers(search, sortBy, sortDirection, statusFilter, emailFilter, roleFilter);

            ViewBag.TotalCount = db.Users.Count();
            ViewBag.FilteredCount = users.Count;

            ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) ||
                                      statusFilter != "all" ||
                                      emailFilter != "all" ||
                                      roleFilter != "all";

            return View(users);
        }

        private List<UserViewModel> GetFilteredUsers(string search, string sortBy, string sortDirection,
        string statusFilter, string emailFilter, string roleFilter)
        {
            var query = from u in db.Users
                        join ura in db.UserRoleAssignments on u.Id equals ura.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join r in db.UserRoles on ur.RoleId equals r.Id into roles
                        from role in roles.DefaultIfEmpty()
                        where ur == null || ur.IsActive
                        select new UserViewModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            ProfilePicture = u.ProfilePicture,
                            JoinDate = u.JoinDate,
                            LastLoginDate = u.LastLoginDate,
                            IsActive = u.IsActive,
                            EmailVerified = u.EmailVerified,
                            RoleName = role == null ? "No Role" : role.Name,
                            SelectedRoleId = ur == null ? 0 : ur.RoleId
                        };

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                    (u.FirstName + " " + u.LastName).ToLower().Contains(search)
                );
            }

            // Apply status filter
            if (statusFilter != "all")
            {
                bool isActive = statusFilter == "active";
                query = query.Where(u => u.IsActive == isActive);
            }

            // Apply email verification filter
            if (emailFilter != "all")
            {
                bool isVerified = emailFilter == "verified";
                query = query.Where(u => u.EmailVerified == isVerified);
            }

            // Apply role filter
            if (roleFilter != "all")
            {
                if (roleFilter == "norole")
                {
                    query = query.Where(u => u.RoleName == "No Role");
                }
                else
                {
                    query = query.Where(u => u.RoleName.ToLower() == roleFilter.ToLower());
                }
            }

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "id":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id);
                    break;
                case "username":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username);
                    break;
                case "email":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                    break;
                case "firstname":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName);
                    break;
                case "lastname":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName);
                    break;
                case "lastlogin":
                    query = sortDirection == "asc" ? query.OrderBy(u => u.LastLoginDate) : query.OrderByDescending(u => u.LastLoginDate);
                    break;
                case "created":
                default:
                    query = sortDirection == "asc" ? query.OrderBy(u => u.JoinDate) : query.OrderByDescending(u => u.JoinDate);
                    break;
            }

            return query.ToList();
        }

        #region Create User Controller 
        public ActionResult CreateUser()
        {
            var model = new UserViewModel();
            model.AvailableRoles = GetAllRoles();
            ViewBag.Roles = GetAllRoles();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(UserViewModel model, HttpPostedFileBase profileImage)
        {
            // Additional validation for confirm password
            if (!string.IsNullOrEmpty(model.Password) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
            }

            // Validate password strength (optional)
            if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            // Check if username already exists
                            if (db.Users.Any(u => u.Username.ToLower() == model.Username.ToLower()))
                            {
                                ModelState.AddModelError("Username", "Username already exists. Please choose a different username.");
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            // Check if email already exists
                            if (db.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                            {
                                ModelState.AddModelError("Email", "Email already exists. Please use a different email address.");
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            // Create the user
                            var user = new User
                            {
                                Username = model.Username ?? string.Empty,
                                Email = model.Email ?? string.Empty,
                                PasswordHash = model.Password ?? string.Empty,
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                ProfilePicture = model.ProfilePicture, // Keep URL fallback
                                IsActive = model.IsActive,
                                EmailVerified = model.EmailVerified,
                                JoinDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            // Save profile image as binary data
                            try
                            {
                                SaveProfileImageToDatabase(user, profileImage);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("ProfilePicture", ex.Message);
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            db.Users.Add(user);
                            db.SaveChanges();

                            // Assign role if selected
                            if (model.SelectedRoleId > 0)
                            {
                                var roleAssignment = new UserRoleAssignment
                                {
                                    UserId = user.Id,
                                    RoleId = model.SelectedRoleId,
                                    AssignedBy = 1, // Current admin user ID
                                    IsActive = true,
                                    AssignedDate = DateTime.Now
                                };

                                db.UserRoleAssignments.Add(roleAssignment);
                                db.SaveChanges();

                                CreateUserTypeRecord(user.Id, model.SelectedRoleId);
                            }

                            transaction.Commit();
                            TempData["Success"] = "User created successfully!";
                            return RedirectToAction("User_Manager");
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error creating user: " + ex.Message;
                }
            }

            // If we got this far, something failed, redisplay form
            model.AvailableRoles = GetAllRoles();
            ViewBag.Roles = GetAllRoles();
            return View(model);
        }
        #endregion

        #region Edit User Controller
        public ActionResult EditUser(int id)
        {
            UserViewModel user = GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("User_Manager");
            }

            user.AvailableRoles = GetAllRoles();
            ViewBag.Roles = GetAllRoles();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(UserViewModel model, HttpPostedFileBase profileImage)
        {
            try
            {
                // Handle password validation in controller
                if (!ValidatePasswordInController(model, true))
                {
                    model.AvailableRoles = GetAllRoles();
                    ViewBag.Roles = GetAllRoles();
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var user = db.Users.Find(model.Id);
                            if (user == null)
                            {
                                TempData["Error"] = "User not found!";
                                return RedirectToAction("User_Manager");
                            }

                            // Check duplicates
                            if (db.Users.Any(u => u.Username.ToLower() == model.Username.ToLower() && u.Id != model.Id))
                            {
                                ModelState.AddModelError("Username", "Username already exists.");
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            if (db.Users.Any(u => u.Email.ToLower() == model.Email.ToLower() && u.Id != model.Id))
                            {
                                ModelState.AddModelError("Email", "Email already exists.");
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            // Update user properties
                            user.Username = model.Username ?? user.Username ?? string.Empty;
                            user.Email = model.Email ?? user.Email ?? string.Empty;
                            user.FirstName = model.FirstName ?? user.FirstName;
                            user.LastName = model.LastName ?? user.LastName;
                            user.IsActive = model.IsActive;
                            user.EmailVerified = model.EmailVerified;
                            user.UpdatedAt = DateTime.Now;

                            // Handle profile picture upload (binary)
                            try
                            {
                                SaveProfileImageToDatabase(user, profileImage);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("ProfilePicture", ex.Message);
                                model.AvailableRoles = GetAllRoles();
                                ViewBag.Roles = GetAllRoles();
                                return View(model);
                            }

                            // If no file uploaded but URL is provided, save URL as fallback
                            if (profileImage == null || profileImage.ContentLength == 0)
                            {
                                if (!string.IsNullOrEmpty(model.ProfilePicture) && model.ProfilePicture.Trim() != "")
                                {
                                    user.ProfilePicture = model.ProfilePicture.Trim();
                                }
                            }

                            if (!string.IsNullOrEmpty(model.Password))
                            {
                                user.PasswordHash = model.Password;
                            }

                            db.SaveChanges();

                            // Handle role assignments without duplicate key error
                            HandleRoleAssignment(model.Id, model.SelectedRoleId);

                            transaction.Commit();
                            TempData["Success"] = "User updated successfully!";
                            return RedirectToAction("User_Manager");
                        }
                        catch (Exception innerEx)
                        {
                            transaction.Rollback();
                            var fullError = GetFullExceptionMessage(innerEx);
                            TempData["Error"] = "Database error updating user: " + fullError;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var fullError = GetFullExceptionMessage(ex);
                TempData["Error"] = "Error updating user: " + fullError;
            }

            // Reload user data if validation fails
            var refreshedUser = GetUserById(model.Id);
            if (refreshedUser != null)
            {
                // Keep the user's current profile picture info
                model.ProfilePicture = refreshedUser.ProfilePicture;
            }

            model.AvailableRoles = GetAllRoles();
            ViewBag.Roles = GetAllRoles();
            return View(model);
        }
        #endregion

        private void HandleRoleAssignment(int userId, int newRoleId)
        {
            try
            {
                // Get existing active role assignments for this user
                var existingAssignments = db.UserRoleAssignments
                    .Where(x => x.UserId == userId && x.IsActive)
                    .ToList();

                // Check if the user already has this exact role assignment
                var existingRole = existingAssignments.FirstOrDefault(x => x.RoleId == newRoleId);

                if (existingRole != null && newRoleId > 0)
                {
                    // User already has this role, no need to do anything
                    return;
                }

                // Deactivate all existing role assignments
                foreach (var assignment in existingAssignments)
                {
                    assignment.IsActive = false;
                }

                // Add new role assignment if a role is selected
                if (newRoleId > 0)
                {
                    var newAssignment = new UserRoleAssignment
                    {
                        UserId = userId,
                        RoleId = newRoleId,
                        AssignedBy = 1, // Current admin user ID
                        IsActive = true,
                        AssignedDate = DateTime.Now
                    };

                    db.UserRoleAssignments.Add(newAssignment);
                }

                db.SaveChanges();

                // Update user type records
                if (newRoleId > 0)
                {
                    UpdateUserTypeRecord(userId, newRoleId);
                }
                else
                {
                    RemoveUserTypeRecords(userId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleRoleAssignment: {ex.Message}");
                throw;
            }
        }

        private bool ValidatePasswordInController(UserViewModel model, bool isEdit = false)
        {
            // Remove password validation for edit if password is empty (means don't change password)
            if (isEdit && string.IsNullOrEmpty(model.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
                return true;
            }

            // Validate password and confirmation
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                    return false;
                }

                if (model.Password.Length < 6)
                {
                    ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
                    return false;
                }
            }
            else if (!isEdit)
            {
                ModelState.AddModelError("Password", "Password is required.");
                return false;
            }

            return true;
        }


        private void SaveProfileImageToDatabase(User user, HttpPostedFileBase profileImage)
        {
            if (profileImage != null && profileImage.ContentLength > 0)
            {
                // Validate file size (5MB max)
                if (profileImage.ContentLength > 5 * 1024 * 1024)
                {
                    throw new Exception("Profile picture file size cannot exceed 5MB.");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(profileImage.ContentType.ToLower()))
                {
                    throw new Exception("Only JPEG, PNG, GIF, and WebP image formats are allowed for profile pictures.");
                }

                // Save binary data to database
                using (var binaryReader = new System.IO.BinaryReader(profileImage.InputStream))
                {
                    user.ProfilePictureData = binaryReader.ReadBytes(profileImage.ContentLength);
                    user.ProfilePictureContentType = profileImage.ContentType;
                    user.ProfilePictureFileName = profileImage.FileName;
                }
            }
        }

        [HttpGet]
        public ActionResult GetProfilePicture(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user?.ProfilePictureData != null && user.ProfilePictureData.Length > 0)
                {
                    return File(user.ProfilePictureData, user.ProfilePictureContentType ?? "image/jpeg");
                }

                // Fallback to URL-based profile picture if no binary data
                if (!string.IsNullOrEmpty(user?.ProfilePicture) && !user.ProfilePicture.StartsWith("~/Uploads/"))
                {
                    // For external URLs, redirect to them
                    return Redirect(user.ProfilePicture);
                }

                // Return a default "no image" placeholder
                string defaultImagePath = Server.MapPath("~/Content/images/default-avatar.png");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    return File(defaultImagePath, "image/png");
                }

                return new HttpStatusCodeResult(404);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500);
            }
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            var message = ex.Message;
            if (ex.InnerException != null)
            {
                message += " Inner Exception: " + ex.InnerException.Message;
                if (ex.InnerException.InnerException != null)
                {
                    message += " Inner Inner Exception: " + ex.InnerException.InnerException.Message;
                }
            }
            return message;
        }

        [HttpGet]
        public JsonResult GetCurrentPassword(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null)
                {
                    // For security, we'll return a placeholder showing password exists
                    return Json(new { success = true, hasPassword = !string.IsNullOrEmpty(user.PasswordHash) }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewUser(int id)
        {
            UserViewModel user = GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("User_Manager");
            }
            return View(user);
        }

        #region Delete Controller 
        public ActionResult DeleteUser(int id)
        {
            UserViewModel user = GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("User_Manager");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null)
                {
                    // Remove related records first (due to foreign key constraints)
                    var roleAssignments = db.UserRoleAssignments.Where(x => x.UserId == id).ToList();
                    db.UserRoleAssignments.RemoveRange(roleAssignments);

                    // Remove user type records
                    RemoveUserTypeRecords(id);

                    // Remove the user
                    db.Users.Remove(user);
                    db.SaveChanges();

                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "User not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting user: " + ex.Message;
            }
            return RedirectToAction("User_Manager");
        }
        #endregion

        [HttpGet]
        public JsonResult GetUserPassword(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null && !string.IsNullOrEmpty(user.PasswordHash))
                {
                    return Json(new
                    {
                        success = true,
                        password = user.PasswordHash,
                        hasPassword = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    success = false,
                    hasPassword = false,
                    message = "No password found"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    hasPassword = false,
                    message = "Error retrieving password: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ToggleActive(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    user.UpdatedAt = DateTime.Now;
                    db.SaveChanges();
                    TempData["Success"] = "User status updated successfully!";
                }
                else
                {
                    TempData["Error"] = "User not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating user status: " + ex.Message;
            }
            return RedirectToAction("User_Manager");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<UserViewModel> GetAllUsers()
        {
            var users = from u in db.Users
                        join ura in db.UserRoleAssignments on u.Id equals ura.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join r in db.UserRoles on ur.RoleId equals r.Id into roles
                        from role in roles.DefaultIfEmpty()
                        where ur == null || ur.IsActive
                        orderby u.JoinDate descending
                        select new UserViewModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            ProfilePicture = u.ProfilePicture,
                            JoinDate = u.JoinDate,
                            LastLoginDate = u.LastLoginDate,
                            IsActive = u.IsActive,
                            EmailVerified = u.EmailVerified,
                            RoleName = role == null ? "No Role" : role.Name,
                            SelectedRoleId = ur == null ? 0 : ur.RoleId
                        };

            return users.ToList();
        }

        private UserViewModel GetUserById(int id)
        {
            var user = (from u in db.Users
                        join ura in db.UserRoleAssignments on u.Id equals ura.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join r in db.UserRoles on ur.RoleId equals r.Id into roles
                        from role in roles.DefaultIfEmpty()
                        where u.Id == id && (ur == null || ur.IsActive)
                        select new UserViewModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            ProfilePicture = u.ProfilePicture,
                            JoinDate = u.JoinDate,
                            LastLoginDate = u.LastLoginDate,
                            IsActive = u.IsActive,
                            EmailVerified = u.EmailVerified,
                            RoleName = role == null ? "No Role" : role.Name,
                            SelectedRoleId = ur == null ? 0 : ur.RoleId
                        }).FirstOrDefault();

            return user;
        }

        private List<RoleViewModel> GetAllRoles()
        {
            var roles = db.UserRoles
                        .OrderBy(r => r.Name)
                        .Select(r => new RoleViewModel
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Description = r.Description
                        }).ToList();

            return roles;
        }

        private void CreateUserTypeRecord(int userId, int roleId)
        {
            try
            {
                var role = db.UserRoles.Find(roleId);
                if (role == null) return;

                switch (role.Name.ToLower())
                {
                    case "reader":
                        try
                        {
                            var reader = new Reader { UserId = userId };
                            db.Readers.Add(reader);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating reader record: {ex.Message}");
                        }
                        break;
                    
                    case "author":
                        try
                        {
                            var author = new Author { UserId = userId, PenName = "New Author" };
                            db.Authors.Add(author);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating author record: {ex.Message}");
                        }
                        break;
                    
                    case "staff":
                        try
                        {
                            var staff = new Staff { UserId = userId, Department = "General", Position = "Staff Member" };
                            db.Staff.Add(staff);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating staff record: {ex.Message}");
                            // Try alternative approach if Staff entity has issues
                            try
                            {
                                db.Database.ExecuteSqlCommand(
                                    "INSERT INTO Staff (UserId, Department, Position) VALUES (@p0, @p1, @p2)",
                                    userId, "General", "Staff Member");
                            }
                            catch (Exception sqlEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error with raw SQL for staff: {sqlEx.Message}");
                            }
                        }
                        break;
                    
                    case "admin":
                        try
                        {
                            var admin = new AdminModel { UserId = userId, AdminLevel = 1 };
                            db.Admins.Add(admin);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating admin record: {ex.Message}");
                        }
                        break;
                }
                
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving user type record: {ex.Message}");
                    // Don't throw - role assignment can work without user type records
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateUserTypeRecord: {ex.Message}");
            }
        }

        private void UpdateUserTypeRecord(int userId, int roleId)
        {
            // Remove existing user type records
            RemoveUserTypeRecords(userId);

            // Create new user type record
            CreateUserTypeRecord(userId, roleId);
        }

        private void RemoveUserTypeRecords(int userId)
        {
            try
            {
                // Remove from all user type tables with individual try-catch blocks
                try
                {
                    var readers = db.Readers.Where(r => r.UserId == userId).ToList();
                    if (readers.Any())
                    {
                        db.Readers.RemoveRange(readers);
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue - table might not exist
                    System.Diagnostics.Debug.WriteLine($"Error removing readers: {ex.Message}");
                }

                try
                {
                    var authors = db.Authors.Where(a => a.UserId == userId).ToList();
                    if (authors.Any())
                    {
                        db.Authors.RemoveRange(authors);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error removing authors: {ex.Message}");
                }

                try
                {
                    // Try different possible naming conventions for Staff table
                    var staffMembers = db.Staff.Where(s => s.UserId == userId).ToList();
                    if (staffMembers.Any())
                    {
                        db.Staff.RemoveRange(staffMembers);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error removing staff: {ex.Message}");
                    // If Staff table doesn't exist, try alternative approaches
                    try
                    {
                        // Alternative: Execute raw SQL if needed
                        db.Database.ExecuteSqlCommand("DELETE FROM Staff WHERE UserId = @p0", userId);
                    }
                    catch (Exception sqlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error with raw SQL for staff: {sqlEx.Message}");
                        // Continue without error if table doesn't exist
                    }
                }

                try
                {
                    var admins = db.Admins.Where(a => a.UserId == userId).ToList();
                    if (admins.Any())
                    {
                        db.Admins.RemoveRange(admins);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error removing admins: {ex.Message}");
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RemoveUserTypeRecords: {ex.Message}");
                // Don't throw - this is cleanup and shouldn't break the main operation
            }
        }

    }
}