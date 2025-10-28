using DarkNovel.Data;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult BuyPackage(int id)
        {
            var package = _context.CoinPackages.FirstOrDefault(p => p.Id == id && p.IsActive);
            if (package == null)
                return NotFound(new { message = "Gói coin không tồn tại hoặc đã bị vô hiệu hóa." });

            // TODO: Xử lý logic mua coin (sau này thêm user login)
            return Ok(new { message = $"Bạn đã mua gói {package.Name} thành công!" });
        }
    }
}
