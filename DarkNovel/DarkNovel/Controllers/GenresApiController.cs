using DarkNovel.Data;
using Microsoft.AspNetCore.Mvc;
using DarkNovel.Data;
using DarkNovel.Models;
using DarkNovel.Models.ApiModels;

namespace WebNovel.Controllers.Api
{
    [Route("api/genres")]
    [ApiController]
    public class GenresApiController : ControllerBase
    {
        private readonly DarkNovelContext _db;

        public GenresApiController(DarkNovelContext db)
        {
            _db = db;
        }

        // GET: api/genres?search=&activeOnly=true
        [HttpGet]
        [Route("")]
        public IActionResult GetGenres(string search = "", bool activeOnly = true)
        {
            try
            {
                var query = _db.Genres.AsQueryable();

                if (activeOnly)
                    query = query.Where(g => g.IsActive);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(g => g.Name.Contains(search) || g.Description.Contains(search));

                var genres = query
                    .OrderBy(g => g.Name)
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Description,
                        g.IconClass,
                        g.ColorCode,
                        g.IsActive
                    })
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    Data = genres,
                    Count = genres.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving genres: " + ex.Message +
                              (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "")
                });
            }
        }

        // GET: api/genres/5
        [HttpGet("{id:int}")]
        public IActionResult GetGenre(int id)
        {
            try
            {
                var genre = _db.Genres.Find(id);
                if (genre == null)
                    return NotFound(new { Success = false, Message = "Genre not found" });

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        genre.Id,
                        genre.Name,
                        genre.Description,
                        genre.IconClass,
                        genre.ColorCode,
                        genre.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving genre: " + ex.Message
                });
            }
        }
    }
}
