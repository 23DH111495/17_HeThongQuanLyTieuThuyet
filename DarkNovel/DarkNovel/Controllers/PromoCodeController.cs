using DarkNovel.Data;
using DarkNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // ⭐️ Đảm bảo bạn có dòng này
using Microsoft.AspNetCore.Authorization; // ⭐️ Đảm bảo bạn có dòng này

namespace DarkNovel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromoCodeController : ControllerBase
    {
        private readonly DarkNovelContext _context;

        public PromoCodeController(DarkNovelContext context)
        {
            _context = context;
        }


        // 🟢 GET 1: Lấy tất cả mã khuyến mãi (Hữu ích cho Admin)
        // GET: api/PromoCode
        [HttpGet]
        public async Task<IActionResult> GetAllPromos()
        {
            var promos = await _context.PromoCodes
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(promos);
        }

        // 🟡 GET 2: Lấy chi tiết 1 mã theo Code (string)
        // Dùng để user "kiểm tra" thông tin mã trước khi áp dụng
        // GET: api/PromoCode/SUMMER2024
        [HttpGet("{code}")]
        public async Task<IActionResult> GetPromoCode(string code)
        {
            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive == true);

            if (promo == null)
                return NotFound(new { message = "Mã không hợp lệ hoặc đã hết hạn" });

            // Kiểm tra hạn
            if (promo.ValidUntil.HasValue && promo.ValidUntil < DateTime.UtcNow)
                return BadRequest(new { message = "Mã khuyến mãi đã hết hạn" });

            return Ok(new
            {
                code = promo.Code,
                description = promo.Description,
                promoType = promo.PromoType,
                value = promo.Value,
                validUntil = promo.ValidUntil
            });
        }



        [HttpPost("apply")]
        [Authorize] // ⭐️ Bắt buộc user phải đăng nhập
        public async Task<IActionResult> ApplyPromo([FromBody] ApplyPromoRequest request)
        {
            // ⭐️ --- BẮT ĐẦU PHẦN XÁC THỰC MỚI --- ⭐️
            // (Giống hệt CoinPackageController)

            // Lấy TẤT CẢ các claim "NameIdentifier"
            var nameIdentifierClaims = User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .ToList();

            // Tìm claim NÀO LÀ SỐ (đó mới là UserId)
            var userIdClaim = nameIdentifierClaims.FirstOrDefault(c => int.TryParse(c.Value, out _));

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // Nếu không tìm thấy claim nào là số, trả về lỗi
                return Unauthorized(new { message = "Token người dùng không hợp lệ (Không tìm thấy ID)." });
            }
            // ⭐️ --- KẾT THÚC PHẦN XÁC THỰC MỚI --- ⭐️


            // 2. Bắt đầu một Transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3. Tìm mã promo và kiểm tra điều kiện
                var promo = await _context.PromoCodes
                    .FirstOrDefaultAsync(p => p.Code == request.Code && p.IsActive == true);

                if (promo == null)
                    return NotFound(new { message = "Mã khuyến mãi không tồn tại hoặc không hoạt động." });

                // kiểm tra thời gian hiệu lực
                var now = DateTime.UtcNow;
                if (promo.ValidFrom.HasValue && promo.ValidFrom > now)
                    return BadRequest(new { message = "Mã khuyến mãi chưa bắt đầu có hiệu lực." });

                if (promo.ValidUntil.HasValue && promo.ValidUntil < now)
                    return BadRequest(new { message = "Mã khuyến mãi đã hết hạn." });

                // kiểm tra số lượt sử dụng chung
                if (promo.MaxUses.HasValue && promo.UsedCount >= promo.MaxUses)
                    return BadRequest(new { message = "Mã khuyến mãi đã được sử dụng hết lượt." });

                // 4. KIỂM TRA QUAN TRỌNG: User này (UserId) đã dùng mã này chưa?
                var alreadyUsed = await _context.PromoCodeUsage
                    .AnyAsync(u => u.PromoCodeId == promo.Id && u.UserId == userId);

                if (alreadyUsed)
                    return BadRequest(new { message = "Bạn đã sử dụng mã khuyến mãi này rồi." });

                // 5. LOGIC CỘNG COIN (dựa trên UserId)
                if (promo.PromoType == "FreeCoins" && promo.Value > 0)
                {
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                    if (wallet == null)
                    {
                        wallet = new Wallet { UserId = userId, CoinBalance = 0, LastUpdated = now };
                        _context.Wallets.Add(wallet);
                    }

                    decimal balanceBefore = wallet.CoinBalance;
                    wallet.CoinBalance += promo.Value;
                    wallet.TotalCoinsEarned += promo.Value; 
                    wallet.LastUpdated = now;

                    var coinTransaction = new CoinTransaction
                    {
                        UserId = userId,
                        TransactionType = "PromoCode",
                        Amount = promo.Value,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = wallet.CoinBalance,
                        Description = $"Nhận {promo.Value} coins từ mã {promo.Code}",
                        CreatedAt = now
                    };
                    _context.CoinTransactions.Add(coinTransaction);
                }

                // 6. Cập nhật lượt sử dụng của mã
                promo.UsedCount = (promo.UsedCount ?? 0) + 1;

                // 7. Ghi lại lịch sử sử dụng mã (cho user này)
                var usageLog = new PromoCodeUsage
                {
                    PromoCodeId = promo.Id,
                    UserId = userId,
                    CoinsReceived = (promo.PromoType == "FreeCoins" ? promo.Value : 0),
                    UsedDate = now
                };
                _context.PromoCodeUsage.Add(usageLog);

                // 8. Lưu tất cả thay đổi và commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = $"Áp dụng mã thành công! Bạn nhận được {promo.Value} coins.",
                    promo.Code,
                    promo.PromoType,
                    value = promo.Value,
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại.", error = ex.Message });
            }
        }
    }

    public class ApplyPromoRequest
    {
        public string Code { get; set; }
    }
}