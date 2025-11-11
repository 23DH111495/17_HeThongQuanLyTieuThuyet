using DarkNovel.Data;
using DarkNovel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DarkNovel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinPackageController : ControllerBase
    {
        private readonly DarkNovelContext _context;

        public CoinPackageController(DarkNovelContext context)
        {
            _context = context;
        }

        // GET: api/CoinPackage 
        [HttpGet]
        public IActionResult GetAllPackages()
        {
            var packages = _context.CoinPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CoinAmount,
                    p.BonusCoins,
                    p.PriceUSD,
                    p.PriceVND,
                    p.IsFeatured
                }).ToList();

            return Ok(packages);
        }

        // POST: api/CoinPackage/buy/5 
        [HttpPost("buy/{id}")]
        [Authorize]
        public async Task<IActionResult> BuyPackage(int id)
        {
            // Get ALL nameidentifier claims
            var nameIdentifierClaims = User.Claims
                .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                .ToList();

            // Get the one that's a number (the user ID)
            var userIdClaim = nameIdentifierClaims.FirstOrDefault(c => int.TryParse(c.Value, out _));

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine("FAILED: Could not find valid UserId in claims");
                return Unauthorized(new { message = "Token không hợp lệ hoặc không tìm thấy UserId." });
            }

            Console.WriteLine($"SUCCESS: UserId = {userId}, PackageId = {id}");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var package = await _context.CoinPackages.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
                    if (package == null)
                    {
                        Console.WriteLine($"Package not found: {id}");
                        return NotFound(new { message = "Gói coin không tồn tại hoặc đã bị vô hiệu hóa." });
                    }

                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                    if (wallet == null)
                    {
                        wallet = new Wallet { UserId = userId, CoinBalance = 0, TotalCoinsEarned = 0, TotalCoinsSpent = 0, LastUpdated = DateTime.UtcNow, Version = 1 };
                        _context.Wallets.Add(wallet);
                        await _context.SaveChangesAsync();
                    }

                    decimal balanceBefore = wallet.CoinBalance;
                    decimal coinsToAdd = package.CoinAmount + package.BonusCoins;
                    decimal balanceAfter = balanceBefore + coinsToAdd;

                    var purchaseHistory = new CoinPurchaseHistory
                    {
                        UserId = userId,
                        PackageId = package.Id,
                        CoinsPurchased = package.CoinAmount,
                        BonusCoins = package.BonusCoins,
                        PricePaid = package.PriceVND,
                        Currency = "VND",
                        PaymentStatus = "Completed",
                        PaymentMethod = "Test",
                        PaymentDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CoinPurchaseHistories.Add(purchaseHistory);
                    await _context.SaveChangesAsync();

                    var coinTransaction = new CoinTransaction
                    {
                        UserId = userId,
                        TransactionType = "Purchase",
                        Amount = coinsToAdd,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        RelatedPurchaseId = purchaseHistory.Id,
                        Description = $"Mua gói {package.Name}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CoinTransactions.Add(coinTransaction);

                    wallet.CoinBalance = balanceAfter;
                    wallet.TotalCoinsEarned += coinsToAdd;
                    wallet.LastUpdated = DateTime.UtcNow;

                    _context.Wallets.Update(wallet);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"Purchase successful! New balance: {wallet.CoinBalance}");
                    return Ok(wallet);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    return StatusCode(500, new { message = "Đã xảy ra lỗi server. Giao dịch bị hủy.", error = ex.Message });
                }
            }
        }
    }
}