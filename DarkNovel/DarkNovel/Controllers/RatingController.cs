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
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateRating([FromBody] Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == rating.ReaderId);
            if (!userExists)
                return BadRequest(new { message = "User does not exist." });

            // Get or create Reader profile
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.UserId == rating.ReaderId);
            if (reader == null)
            {
                var newReader = new Reader
                {
                    UserId = rating.ReaderId,
                    IsPremium = false,
                    ReadingPreferences = "{}",
                    FavoriteGenres = "[]",
                    NotificationSettings = "{}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Readers.Add(newReader);
                await _context.SaveChangesAsync();
                reader = newReader;
            }

            // Check if novel exists
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == rating.NovelId);
            if (!novelExists)
                return BadRequest(new { message = "Novel does not exist." });

            // Check for existing rating
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ReaderId == reader.Id && r.NovelId == rating.NovelId);

            if (existingRating != null)
            {
                existingRating.RatingValue = rating.RatingValue;
                existingRating.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // ✅ Return simple response without navigation properties
                return Ok(new
                {
                    message = "Rating updated successfully.",
                    rating = new
                    {
                        id = existingRating.Id,
                        readerId = existingRating.ReaderId,
                        novelId = existingRating.NovelId,
                        ratingValue = existingRating.RatingValue,
                        createdAt = existingRating.CreatedAt,
                        updatedAt = existingRating.UpdatedAt
                    }
                });
            }

            // Create new rating
            var newRating = new Rating
            {
                ReaderId = reader.Id,
                NovelId = rating.NovelId,
                RatingValue = rating.RatingValue,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Ratings.Add(newRating);
            await _context.SaveChangesAsync();

            // ✅ Return simple response without navigation properties
            return Ok(new
            {
                message = "Rating created successfully.",
                rating = new
                {
                    id = newRating.Id,
                    readerId = newRating.ReaderId,
                    novelId = newRating.NovelId,
                    ratingValue = newRating.RatingValue,
                    createdAt = newRating.CreatedAt,
                    updatedAt = newRating.UpdatedAt
                }
            });
        }

        // ✅ GET: api/Rating/novel/{novelId}
        [HttpGet("novel/{novelId:int}")]
        public async Task<IActionResult> GetAverageRatingByNovel(int novelId)
        {
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == novelId);
            if (!novelExists)
                return NotFound(new { message = "Novel not found." });

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

            decimal average = ratings.Average(r => r.RatingValue);

            return Ok(new
            {
                novelId,
                averageRating = Math.Round(average, 1),
                totalRatings = ratings.Count
            });
        }

        [HttpGet("list/novel/{novelId:int}")]
        public async Task<IActionResult> GetRatingsByNovel(int novelId)
        {
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == novelId);
            if (!novelExists)
                return NotFound(new { message = "Novel not found." });

            var ratings = await _context.Ratings
                .Where(r => r.NovelId == novelId)
                .Include(r => r.Reader)
                    .ThenInclude(reader => reader.User)
                .Select(r => new
                {
                    r.Id,
                    r.ReaderId,
                    ReaderName = r.Reader != null && r.Reader.User != null
                        ? r.Reader.User.Username
                        : "Unknown",
                    r.NovelId,
                    RatingValue = r.RatingValue,
                    r.CreatedAt,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // ✅ PUT: api/Rating/{id}
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
                rating = new
                {
                    id = existing.Id,
                    readerId = existing.ReaderId,
                    novelId = existing.NovelId,
                    ratingValue = existing.RatingValue,
                    createdAt = existing.CreatedAt,
                    updatedAt = existing.UpdatedAt
                }
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