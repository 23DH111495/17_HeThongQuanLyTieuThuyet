using DarkNovel.Data;
using DarkNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DarkNovel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DarkNovelContext _context;
        private readonly IConfiguration _configuration;

        public UserController(DarkNovelContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ... (Hàm Register và GetUsers giữ nguyên) ...
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
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đăng ký thành công!", user.Username });
        }


        // POST: api/user/login (LOGIC MỚI VỚI TRY-CATCH)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Bước 1: Tìm user
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            var reader = _context.Readers.FirstOrDefault(r => r.UserId == user.Id);
            if (user == null)
            {
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            bool isPasswordValid = false;
            bool needsPasswordUpgrade = false;

            try
            {
                // Bước 2.1: Thử xác thực theo kiểu MỚI (BCrypt)
                // Hàm này sẽ hoạt động với các tài khoản mới, hoặc tài khoản cũ đã được nâng cấp.
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // LỖI: "Invalid salt version"
                // Điều này 99% có nghĩa là user.PasswordHash là một MẬT KHẨU CŨ (plain-text).
                // Bây giờ, chúng ta so sánh nó như plain-text.
                isPasswordValid = (user.PasswordHash == request.Password);
                needsPasswordUpgrade = true; // Cần nâng cấp nếu đăng nhập thành công
            }
            catch (Exception)
            {
                // Các lỗi BCrypt khác (ví dụ: hash hỏng)
                isPasswordValid = false;
            }

            // Bước 3: Kiểm tra kết quả
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            // Bước 4: Nâng cấp mật khẩu nếu cần (cho tài khoản cũ)
            if (needsPasswordUpgrade)
            {
                // Nâng cấp mật khẩu plain-text lên hash
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                _context.Users.Update(user);
                await _context.SaveChangesAsync(); // Lưu hash mới vào CSDL
            }

            // Bước 5: Tạo Token
            var token = GenerateJwtToken(user);

            // Bước 6: Trả về Token
            return Ok(new
            {
                message = "Đăng nhập thành công",
                user.Id,
                user.Username,
                user.Email,
                readerId = reader != null ? reader.Id : 0,
                token = token
            });
        }

        // HÀM TẠO TOKEN (Giữ nguyên)
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
