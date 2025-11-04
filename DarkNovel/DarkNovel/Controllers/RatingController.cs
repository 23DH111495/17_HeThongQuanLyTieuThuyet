using DarkNovel.Data;
using DarkNovel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DarkNovel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly DarkNovelContext _context;

        public RatingController(DarkNovelContext context)
        {
            _context = context;
        }

        // ✅ POST: api/Rating
        // Tạo mới hoặc cập nhật nếu đã tồn tại
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateRating([FromBody] Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Kiểm tra Reader và Novel có tồn tại không
            var readerExists = await _context.Readers.AnyAsync(r => r.Id == rating.ReaderId);
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == rating.NovelId);

            if (!readerExists)
                return BadRequest(new { message = "Reader does not exist." });
            if (!novelExists)
                return BadRequest(new { message = "Novel does not exist." });

            // ✅ Nếu người đọc đã đánh giá truyện này thì cập nhật
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ReaderId == rating.ReaderId && r.NovelId == rating.NovelId);

            if (existingRating != null)
            {
                existingRating.RatingValue = rating.RatingValue;
                existingRating.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Rating updated successfully.",
                    rating = existingRating
                });
            }

            // ✅ Nếu chưa có thì tạo mới
            rating.CreatedAt = DateTime.Now;
            rating.UpdatedAt = DateTime.Now;

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Rating created successfully.",
                rating
            });
        }
        // ✅ GET: api/Rating/novel/{novelId}
        [HttpGet("novel/{novelId:int}")]
        public async Task<IActionResult> GetAverageRatingByNovel(int novelId)
        {
            // Kiểm tra truyện có tồn tại không
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == novelId);
            if (!novelExists)
                return NotFound(new { message = "Novel not found." });

            // Lấy danh sách rating của truyện
            var ratings = await _context.Ratings
                .Where(r => r.NovelId == novelId)
                .ToListAsync();

            if (!ratings.Any())
                return Ok(new
                {
                    novelId,
                    averageRating = 0.0,
                    totalRatings = 0
                });

            // Tính trung bình
            decimal average = ratings.Average(r => r.RatingValue);

            return Ok(new
            {
                novelId,
                averageRating = Math.Round(average, 1), // Làm tròn 1 chữ số thập phân
                totalRatings = ratings.Count
            });
        }
        [HttpGet("list/novel/{novelId:int}")]
        public async Task<IActionResult> GetRatingsByNovel(int novelId)
        {
            // Kiểm tra truyện có tồn tại không
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == novelId);
            if (!novelExists)
                return NotFound(new { message = "Novel not found." });

            // Lấy tất cả đánh giá
            var ratings = await _context.Ratings
                .Where(r => r.NovelId == novelId)
                .Include(r => r.Reader) // Nếu muốn lấy thông tin người đánh giá
                .Select(r => new
                {
                    r.Id,
                    r.ReaderId,
                    ReaderName = r.Reader != null ? r.Reader.User != null ? r.Reader.User.Username : "Unknown" : "Unknown",
                    r.NovelId,
                    r.RatingValue,
                    r.CreatedAt,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // ✅ PUT: api/Rating/{id}
        // Cập nhật giá trị đánh giá
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] Rating rating)
        {
            if (id != rating.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await _context.Ratings.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Rating not found." });

            existing.RatingValue = rating.RatingValue;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Rating updated successfully.",
                rating = existing
            });
        }

        // ✅ DELETE: api/Rating/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
                return NotFound(new { message = "Rating not found." });

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating deleted successfully." });
        }
    }
}
