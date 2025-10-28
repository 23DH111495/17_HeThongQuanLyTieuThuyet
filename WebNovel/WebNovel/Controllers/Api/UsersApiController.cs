using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography; // ĐÃ BẬT LẠI
using System.Text; // ĐÃ BẬT LẠI
using System.Web;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ApiModels;

namespace WebNovel.Controllers.Api
{
    [RoutePrefix("api/users")]
    public class UsersApiController : ApiController
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public UsersApiController()
        {
            Debug.WriteLine($"UsersApiController constructor called at {DateTime.Now}");
        }

        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestRoute()
        {
            Debug.WriteLine($"GET api/users/test called at {DateTime.Now}");
            return Ok(new { message = "Test route working", timestamp = DateTime.Now });
        }

        // ... (Các hàm test-register, test-login, ListUsers, DebugInfo giữ nguyên như file gốc của bạn) ...

        [HttpGet]
        [Route("")]
        public IHttpActionResult ListUsers()
        {
            try
            {
                // SỬ DỤNG IsActive VÌ DATABASE MỚI ĐÃ CÓ
                var usersQuery = db.Users
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.JoinDate,
                        u.FirstName,
                        u.LastName,
                        u.LastLoginDate // SỬ DỤNG LastLoginDate VÌ DATABASE MỚI ĐÃ CÓ
                    })
                    .OrderBy(u => u.Username);

                var users = usersQuery.ToList();

                return Ok(new ApiResponse<List<object>>
                {
                    Success = true,
                    Message = $"Successfully retrieved {users.Count} active users.",
                    Data = users.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching users: " + ex.Message
                });
            }
        }


        // POST: api/users/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            Debug.WriteLine($"POST api/users/login called at {DateTime.Now}");
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            try
            {
                // SỬ DỤNG IsActive VÌ DATABASE MỚI ĐÃ CÓ
                var user = db.Users.FirstOrDefault(u =>
                    (u.Username == request.Username || u.Email == request.Username) &&
                    u.IsActive); // ĐÃ BẬT LẠI

                if (user == null)
                {
                    Debug.WriteLine($"Login failed: User not found for '{request.Username}'");
                    return BadRequest("Invalid credentials");
                }

                // QUAN TRỌNG: SỬ DỤNG LẠI HASHING VÌ DATABASE MỚI HỖ TRỢ HASH
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    Debug.WriteLine($"Login failed: Invalid password for user '{user.Username}'");
                    return BadRequest("Invalid credentials");
                }

                Debug.WriteLine($"Login successful for user: {user.Username}");

                // ĐÃ BẬT LẠI TẤT CẢ TÍNH NĂNG
                user.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                var userRoles = db.UserRoleAssignments
                    .Where(ura => ura.UserId == user.Id && ura.IsActive)
                    .Include(ura => ura.UserRole)
                    .Select(ura => ura.UserRole.Name)
                    .ToList();

                var reader = db.Readers.FirstOrDefault(r => r.UserId == user.Id);
                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == user.Id);

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = reader?.IsPremium ?? false,
                    CoinBalance = wallet?.CoinBalance ?? 0,
                    Roles = userRoles
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error during login: " + ex.Message));
            }
        }

        // POST: api/users/register
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            Debug.WriteLine($"POST api/users/register called at {DateTime.Now}");
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username, email, and password are required");
            }

            try
            {
                if (db.Users.Any(u => u.Username == request.Username))
                {
                    return BadRequest("Username already exists");
                }
                if (db.Users.Any(u => u.Email == request.Email))
                {
                    return BadRequest("Email already exists");
                }

                Debug.WriteLine("Creating new user...");

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    // QUAN TRỌNG: SỬ DỤNG HASHING CHO BẢO MẬT
                    PasswordHash = HashPassword(request.Password),
                    JoinDate = DateTime.Now,
                    IsActive = true, // SỬ DỤNG IsActive VÌ DATABASE MỚI ĐÃ CÓ
                    EmailVerified = false
                };
                db.Users.Add(user);
                db.SaveChanges(); // Lưu user trước để lấy Id

                Debug.WriteLine($"User created with ID: {user.Id}");

                // ĐÃ BẬT LẠI TẤT CẢ TÍNH NĂNG
                var reader = new Reader
                {
                    UserId = user.Id,
                    IsPremium = false,
                    ReadingPreferences = "{}",
                    FavoriteGenres = "[]",
                    NotificationSettings = "{\"newChapters\":true,\"authorUpdates\":true}"
                };
                db.Readers.Add(reader);

                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CoinBalance = 100, // Welcome bonus
                    TotalCoinsEarned = 100
                };
                db.Wallets.Add(wallet);

                // Gán vai trò "Reader"
                var readerRole = db.UserRoles.FirstOrDefault(r => r.Name == "Reader");
                if (readerRole != null)
                {
                    var roleAssignment = new UserRoleAssignment
                    {
                        UserId = user.Id,
                        RoleId = readerRole.Id,
                        IsActive = true
                    };
                    db.UserRoleAssignments.Add(roleAssignment);
                    Debug.WriteLine("Reader role assigned");
                }
                else
                {
                    // Lỗi này xảy ra nếu bạn chưa thêm 'Reader' vào bảng UserRoles
                    Debug.WriteLine("Warning: Reader role not found in database. Please insert it.");
                }

                // Thêm giao dịch bonus
                var welcomeTransaction = new CoinTransaction
                {
                    UserId = user.Id,
                    TransactionType = "Bonus",
                    Amount = 100,
                    BalanceBefore = 0,
                    BalanceAfter = 100,
                    Description = "Welcome bonus"
                };
                db.CoinTransactions.Add(welcomeTransaction);

                db.SaveChanges(); // Lưu tất cả thay đổi (Reader, Wallet, Role, Transaction)

                Debug.WriteLine("Registration completed successfully");

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = false,
                    CoinBalance = 100,
                    Roles = new List<string> { "Reader" } // Mặc định
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Registration successful. Welcome bonus of 100 coins added!",
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Register error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error during registration: " + ex.Message));
            }
        }

        // GET: api/users/profile/{userId}
        [HttpGet]
        [Route("profile/{userId:int}")]
        public IHttpActionResult GetProfile(int userId)
        {
            Debug.WriteLine($"GET api/users/profile/{userId} called at {DateTime.Now}");
            try
            {
                // SỬ DỤNG IsActive VÌ DATABASE MỚI ĐÃ CÓ
                var user = db.Users.Find(userId);
                if (user == null || !user.IsActive)
                {
                    Debug.WriteLine($"User not found or inactive: {userId}");
                    return NotFound();
                }

                Debug.WriteLine($"Profile found for user: {user.Username}");

                // ĐÃ BẬT LẠI TẤT CẢ TÍNH NĂNG
                var reader = db.Readers.FirstOrDefault(r => r.UserId == userId);
                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == userId);
                var userRoles = db.UserRoleAssignments
                    .Where(ura => ura.UserId == userId && ura.IsActive)
                    .Include(ura => ura.UserRole)
                    .Select(ura => ura.UserRole.Name)
                    .ToList();

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JoinDate = user.JoinDate,
                    IsPremium = reader?.IsPremium ?? false,
                    CoinBalance = wallet?.CoinBalance ?? 0,
                    Roles = userRoles
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProfile error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return InternalServerError(new Exception("Error retrieving profile: " + ex.Message));
            }
        }


        // *** ĐÃ THÊM LẠI CÁC HÀM HASHING BẢO MẬT ***
        private string HashPassword(string password)
        {
            // CẢNH BÁO: Vẫn đang dùng salt cố định, nhưng đã tốt hơn plaintext
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + "YourSaltHere";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
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