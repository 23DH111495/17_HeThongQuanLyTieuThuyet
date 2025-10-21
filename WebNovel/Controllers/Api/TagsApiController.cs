using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Controllers.Api
{
    [RoutePrefix("api/tags")]
    public class TagsApiController : ApiController
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: api/tags - For mobile app (active tags only)
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTags([FromUri] string search = "", [FromUri] bool activeOnly = true)
        {
            try
            {
                // Log the incoming parameters for debugging
                System.Diagnostics.Debug.WriteLine($"GetTags called with search='{search}', activeOnly={activeOnly}");

                var query = db.Tags.AsQueryable();

                // For mobile app, usually only show active tags
                if (activeOnly)
                {
                    query = query.Where(t => t.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
                }

                var tags = query
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Color = t.Color,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt
                    })
                    .ToList();

                var response = new
                {
                    Success = true,
                    Data = tags,
                    Count = tags.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Better error logging
                System.Diagnostics.Debug.WriteLine($"Error in GetTags: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                return InternalServerError(new Exception("Error retrieving tags: " + ex.Message));
            }
        }

        // GET: api/tags/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetTag(int id)
        {
            try
            {
                var tag = db.Tags.Find(id);

                if (tag == null)
                {
                    return NotFound();
                }

                var response = new
                {
                    Success = true,
                    Data = new List<object>
                    {
                        new
                        {
                            Id = tag.Id,
                            Name = tag.Name,
                            Description = tag.Description,
                            Color = tag.Color,
                            IsActive = tag.IsActive,
                            CreatedAt = tag.CreatedAt
                        }
                    },
                    Count = 1
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTag: {ex.Message}");
                return InternalServerError(new Exception("Error retrieving tag: " + ex.Message));
            }
        }

        // GET: api/tags/novel/5 - Get all tags for a specific novel
        [HttpGet]
        [Route("novel/{novelId:int}")]
        public IHttpActionResult GetTagsByNovel(int novelId)
        {
            try
            {
                var tags = db.NovelTags
                    .Where(nt => nt.NovelId == novelId)
                    .Select(nt => new
                    {
                        Id = nt.Tag.Id,
                        Name = nt.Tag.Name,
                        Description = nt.Tag.Description,
                        Color = nt.Tag.Color,
                        IsActive = nt.Tag.IsActive,
                        CreatedAt = nt.Tag.CreatedAt
                    })
                    .OrderBy(t => t.Name)
                    .ToList();

                var response = new
                {
                    Success = true,
                    Data = tags,
                    Count = tags.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTagsByNovel: {ex.Message}");
                return InternalServerError(new Exception("Error retrieving tags for novel: " + ex.Message));
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