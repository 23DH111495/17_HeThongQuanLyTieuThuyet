using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Controllers.Api
{
    [RoutePrefix("api/genres")]
    public class GenresApiController : ApiController
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: api/genres - For mobile app (active genres only)
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetGenres([FromUri] string search = "", [FromUri] bool activeOnly = true)
        {
            try
            {
                // Log the incoming parameters for debugging
                System.Diagnostics.Debug.WriteLine($"GetGenres called with search='{search}', activeOnly={activeOnly}");

                var query = db.Genres.AsQueryable();

                // For mobile app, usually only show active genres
                if (activeOnly)
                {
                    query = query.Where(g => g.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(g => g.Name.Contains(search) || g.Description.Contains(search));
                }

                var genres = query
                    .OrderBy(g => g.Name)
                    .Select(g => new
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        IconClass = g.IconClass,
                        ColorCode = g.ColorCode,
                        IsActive = g.IsActive
                    })
                    .ToList();

                var response = new
                {
                    Success = true,
                    Data = genres,
                    Count = genres.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Better error logging
                System.Diagnostics.Debug.WriteLine($"Error in GetGenres: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                return InternalServerError(new Exception("Error retrieving genres: " + ex.Message));
            }
        }

        // GET: api/genres/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetGenre(int id)
        {
            try
            {
                var genre = db.Genres.Find(id);
                if (genre == null)
                {
                    return NotFound();
                }

                var response = new
                {
                    Success = true,
                    Data = new
                    {
                        Id = genre.Id,
                        Name = genre.Name,
                        Description = genre.Description,
                        IconClass = genre.IconClass,
                        ColorCode = genre.ColorCode,
                        IsActive = genre.IsActive
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetGenre: {ex.Message}");
                return InternalServerError(new Exception("Error retrieving genre: " + ex.Message));
            }
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