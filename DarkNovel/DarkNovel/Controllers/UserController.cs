using DarkNovel.Data;
using DarkNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DarkNovel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DarkNovelContext _context;

        public UserController(DarkNovelContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("Username đã tồn tại!");

            user.JoinDate = DateTime.Now;
            user.IsActive = true;
            user.EmailVerified = false;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Mã hoá mật khẩu

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký thành công!", user.Username });
        }

        // POST: api/user/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            return Ok(new
            {
                message = "Đăng nhập thành công",
                user.Id,
                user.Username,
                user.Email
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

