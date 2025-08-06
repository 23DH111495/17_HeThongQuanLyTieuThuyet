using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Controllers
{
    public class AccountController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            // Check if user is already logged in
            if (Session["IsLoggedIn"] != null && (bool)Session["IsLoggedIn"])
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u =>
                    (u.Username == model.EmailOrUsername || u.Email == model.EmailOrUsername)
                    && u.IsActive);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    if (!user.EmailVerified)
                    {
                        ViewBag.Error = "Please verify your email address before logging in.";
                        return View(model);
                    }

                    // Update last login date
                    user.LastLoginDate = DateTime.Now;
                    user.UpdatedAt = DateTime.Now;
                    db.SaveChanges();

                    // Set session or authentication cookie here
                    Session["UserId"] = user.Id;
                    Session["Username"] = user.Username;
                    Session["IsLoggedIn"] = true;

                    // Handle remember me functionality
                    if (model.RememberMe)
                    {
                        var cookie = new HttpCookie("RememberMe", user.Id.ToString())
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = Request.IsSecureConnection
                        };
                        Response.Cookies.Add(cookie);
                    }

                    // Check user role and redirect accordingly
                    return RedirectBasedOnRole(user.Id);
                }
                else
                {
                    ViewBag.Error = "Invalid email/username or password.";
                }
            }

            return View(model);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            // Check if user is already logged in
            if (Session["IsLoggedIn"] != null && (bool)Session["IsLoggedIn"])
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            try
            {
                // Remove password toggle fields from ModelState if they exist
                ModelState.Remove("ShowPassword");
                ModelState.Remove("ShowConfirmPassword");

                if (ModelState.IsValid)
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(model.Username))
                    {
                        ViewBag.Error = "Username is required.";
                        return View(model);
                    }

                    if (string.IsNullOrWhiteSpace(model.Email))
                    {
                        ViewBag.Error = "Email is required.";
                        return View(model);
                    }

                    // Basic email format validation
                    if (!IsValidEmail(model.Email))
                    {
                        ViewBag.Error = "Please enter a valid email address.";
                        return View(model);
                    }

                    // REMOVED: Password required validation
                    // Allow empty passwords for simplified registration

                    // REMOVED: Password confirmation check
                    // Users can now register with any password or no password

                    if (!model.AgreeToTerms)
                    {
                        ViewBag.Error = "You must agree to the Terms of Service and Privacy Policy.";
                        return View(model);
                    }

                    // Check if username or email already exists
                    if (db.Users.Any(u => u.Username.ToLower() == model.Username.ToLower()))
                    {
                        ViewBag.Error = "Username already exists. Please choose a different username.";
                        return View(model);
                    }

                    if (db.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                    {
                        ViewBag.Error = "Email already exists. Please use a different email address.";
                        return View(model);
                    }

                    // Create new user
                    var user = new User
                    {
                        Username = model.Username.Trim(),
                        Email = model.Email.ToLower().Trim(),
                        // Allow empty/null passwords - store as empty string if null
                        PasswordHash = HashPassword(model.Password ?? string.Empty),
                        FirstName = !string.IsNullOrWhiteSpace(model.FirstName) ? model.FirstName.Trim() : null,
                        LastName = !string.IsNullOrWhiteSpace(model.LastName) ? model.LastName.Trim() : null,
                        JoinDate = DateTime.Now,
                        IsActive = true,
                        EmailVerified = true, // Set to true for simplicity in academic project
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.Users.Add(user);

                    // Save user first to get the ID
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception saveEx)
                    {
                        ViewBag.Error = "Error creating user account. Please try again.";
                        System.Diagnostics.Debug.WriteLine($"User save error: {saveEx.Message}");
                        return View(model);
                    }

                    // Create Reader profile and assign role using User_Manager pattern
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Starting role assignment for user ID: {user.Id}, Username: {user.Username}");

                        // Get Reader role (ID should be 1 based on your screenshot)
                        var readerRole = db.UserRoles.FirstOrDefault(r => r.Name == "Reader");
                        if (readerRole == null)
                        {
                            throw new Exception("Reader role not found in database");
                        }

                        System.Diagnostics.Debug.WriteLine($"Found Reader role with ID: {readerRole.Id}");

                        // First assign the role
                        var roleAssignment = new UserRoleAssignment
                        {
                            UserId = user.Id,
                            RoleId = readerRole.Id,
                            AssignedDate = DateTime.Now,
                            IsActive = true
                            // AssignedBy is nullable, so we're not setting it
                        };

                        System.Diagnostics.Debug.WriteLine($"Adding role assignment to context: UserId={user.Id}, RoleId={readerRole.Id}");
                        db.UserRoleAssignments.Add(roleAssignment);

                        // Save role assignment first
                        db.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("Role assignment saved successfully");

                        // Then create Reader profile using the same method as User_Manager
                        CreateUserTypeRecord(user.Id, readerRole.Id);

                        System.Diagnostics.Debug.WriteLine($"Successfully created Reader profile and assigned Reader role to user {user.Username}");
                    }
                    catch (Exception roleEx)
                    {
                        // Log detailed error information
                        System.Diagnostics.Debug.WriteLine($"=== ROLE ASSIGNMENT FAILED ===");
                        System.Diagnostics.Debug.WriteLine($"Error: {roleEx.Message}");
                        if (roleEx.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner exception: {roleEx.InnerException.Message}");
                            if (roleEx.InnerException.InnerException != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Nested inner exception: {roleEx.InnerException.InnerException.Message}");
                            }
                        }

                        // Check what entities are in the change tracker
                        System.Diagnostics.Debug.WriteLine("Change tracker state:");
                        foreach (var entry in db.ChangeTracker.Entries())
                        {
                            System.Diagnostics.Debug.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                        }

                        // Clear any pending changes that might have failed
                        foreach (var entry in db.ChangeTracker.Entries())
                        {
                            if (entry.Entity is Reader || entry.Entity is UserRoleAssignment)
                            {
                                entry.State = System.Data.Entity.EntityState.Detached;
                                System.Diagnostics.Debug.WriteLine($"Detached {entry.Entity.GetType().Name}");
                            }
                        }

                        // Set error message but continue with registration
                        ViewBag.Error = "Account created but role assignment failed. Please contact support.";
                        System.Diagnostics.Debug.WriteLine($"User {user.Username} created without role assignment");
                    }

                    // Auto-login after registration
                    Session["UserId"] = user.Id;
                    Session["Username"] = user.Username;
                    Session["IsLoggedIn"] = true;

                    // Set welcome message
                    TempData["WelcomeMessage"] = $"Welcome to DarkNovel, {user.FirstName ?? user.Username}! Your account has been created successfully.";

                    // Check user role and redirect accordingly
                    return RedirectBasedOnRole(user.Id);
                }
                else
                {
                    // Log ModelState errors for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    ViewBag.Error = "Please correct the following errors: " + string.Join(", ", errors);
                    System.Diagnostics.Debug.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while creating your account. Please try again.";
                // Log the exception in production
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");

                    // Check for common database errors
                    var innerMessage = ex.InnerException.Message.ToLower();
                    if (innerMessage.Contains("duplicate") || innerMessage.Contains("unique"))
                    {
                        ViewBag.Error = "Username or email already exists. Please try different values.";
                    }
                    else if (innerMessage.Contains("foreign key") || innerMessage.Contains("reference"))
                    {
                        ViewBag.Error = "Database reference error. Please contact support.";
                    }
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            // Clear remember me cookie
            if (Request.Cookies["RememberMe"] != null)
            {
                var cookie = new HttpCookie("RememberMe")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Home");
        }

        // Helper method to redirect based on user role
        private ActionResult RedirectBasedOnRole(int userId)
        {
            try
            {
                // Get user roles using the same pattern as User_Manager
                var userRoleInfo = (from u in db.Users
                                    join ura in db.UserRoleAssignments on u.Id equals ura.UserId into userRoles
                                    from ur in userRoles.DefaultIfEmpty()
                                    join r in db.UserRoles on ur.RoleId equals r.Id into roles
                                    from role in roles.DefaultIfEmpty()
                                    where u.Id == userId && (ur == null || ur.IsActive)
                                    select new
                                    {
                                        RoleName = role == null ? "No Role" : role.Name
                                    }).FirstOrDefault();

                if (userRoleInfo != null && userRoleInfo.RoleName != "No Role")
                {
                    // Store role info in session for future use
                    Session["UserRole"] = userRoleInfo.RoleName;

                    // Check if user is Admin or Staff
                    if (userRoleInfo.RoleName == "Admin" || userRoleInfo.RoleName == "Staff")
                    {
                        // Redirect to admin dashboard
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                }

                // Default redirect for regular users
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log error but don't fail login
                System.Diagnostics.Debug.WriteLine($"Role check error: {ex.Message}");

                // Default redirect if role check fails
                return RedirectToAction("Index", "Home");
            }
        }

        // Helper method for email validation
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to create user type records (copied from User_Manager)
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
                            var reader = new Reader
                            {
                                UserId = userId,
                                IsPremium = false,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            db.Readers.Add(reader);
                            System.Diagnostics.Debug.WriteLine("Reader profile created successfully");
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
                        }
                        break;

                    case "admin":
                        try
                        {
                            var admin = new Admin { UserId = userId, AdminLevel = 1 };
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
                    System.Diagnostics.Debug.WriteLine($"User type record for {role.Name} saved successfully");
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

        // Simple password methods for academic project
        private string HashPassword(string password)
        {
            // For academic purposes - just store password as plain text
            // Handle null/empty passwords gracefully
            // NOTE: Never do this in real applications!
            return password ?? string.Empty;
        }

        private bool VerifyPassword(string password, string storedPassword)
        {
            // Simple string comparison for academic project
            // Handle null values to prevent exceptions
            if (password == null) password = string.Empty;
            if (storedPassword == null) storedPassword = string.Empty;

            return password == storedPassword;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}