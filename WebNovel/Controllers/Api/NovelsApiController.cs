using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ApiModels;
using System.Data.Entity;

namespace WebNovel.Controllers.Api
{
    [RoutePrefix("api/novels")]
    public class NovelsApiController : ApiController
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: api/novels - List novels with filtering/pagination
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetNovels(string search = "", string status = "",
            int? genreId = null, string sortBy = "updated", int page = 1, int pageSize = 20)
        {
            try
            {
                // Start with all active novels
                var query = db.Novels.Where(n => n.IsActive);

                // Apply ModerationStatus filter if the property exists and has valid data
                // Use protective approach in case ModerationStatus doesn't exist
                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work - just use IsActive filter
                }

                // Get the filtered novels as a list to work with in memory
                var allNovels = query.ToList();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    allNovels = allNovels.Where(n =>
                        n.Title != null && n.Title.ToLower().Contains(search.ToLower()) ||
                        (n.Synopsis != null && n.Synopsis.ToLower().Contains(search.ToLower()))
                    ).ToList();
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    allNovels = allNovels.Where(n =>
                        n.Status != null && n.Status.ToLower() == status.ToLower()
                    ).ToList();
                }

                // Apply genre filter
                if (genreId.HasValue)
                {
                    var novelsWithGenre = db.NovelGenres
                        .Where(ng => ng.GenreId == genreId.Value)
                        .Select(ng => ng.NovelId)
                        .ToList();

                    allNovels = allNovels.Where(n => novelsWithGenre.Contains(n.Id)).ToList();
                }

                // Apply sorting in memory
                switch (sortBy.ToLower())
                {
                    case "updated":
                        allNovels = allNovels.OrderByDescending(n => n.LastUpdated).ToList();
                        break;
                    case "popular":
                        allNovels = allNovels.OrderByDescending(n => n.ViewCount).ToList();
                        break;
                    case "rating":
                        allNovels = allNovels.OrderByDescending(n => n.AverageRating).ToList();
                        break;
                    case "bookmarks":
                        allNovels = allNovels.OrderByDescending(n => n.BookmarkCount).ToList();
                        break;
                    case "newest":
                        allNovels = allNovels.OrderByDescending(n => n.PublishDate).ToList();
                        break;
                    default:
                        allNovels = allNovels.OrderByDescending(n => n.LastUpdated).ToList();
                        break;
                }

                // Calculate pagination
                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Apply pagination in memory
                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Get additional data separately
                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                // Get authors separately
                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                // Get genres separately
                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                // Build response
                var novels = novelsList.Select(n => new NovelSummaryDto
                {
                    Id = n.Id,
                    Title = n.Title ?? "Untitled",
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 200 ?
                              n.Synopsis.Substring(0, 200) + "..." : n.Synopsis ?? "",
                    Status = n.Status ?? "Unknown",
                    AverageRating = n.AverageRating,
                    TotalChapters = n.TotalChapters,
                    ViewCount = n.ViewCount,
                    IsPremium = n.IsPremium,
                    LastUpdated = n.LastUpdated,
                    Genres = novelGenres.ContainsKey(n.Id) ? novelGenres[n.Id] : new List<string>()
                }).ToList();

                return Ok(new PaginatedApiResponse<List<NovelSummaryDto>>
                {
                    Success = true,
                    Data = novels,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize  
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/5 - Get novel details
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetNovel(int id)
        {
            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == id && n.IsActive);

                if (novel == null)
                {
                    return NotFound();
                }

                // Get author separately
                var author = db.Authors.Find(novel.AuthorId);

                // Get genres separately
                var genres = db.NovelGenres
                    .Where(ng => ng.NovelId == id)
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id,
                          (ng, g) => new { Id = g.Id, Name = g.Name, ColorCode = g.ColorCode })
                    .ToList();

                // Get recent chapters
                var recentChapters = db.Chapters
                    .Where(c => c.NovelId == id && c.IsPublished)
                    .OrderByDescending(c => c.ChapterNumber)
                    .Take(5)
                    .Select(c => new ChapterSummaryDto
                    {
                        Id = c.Id,
                        ChapterNumber = c.ChapterNumber,
                        Title = c.Title,
                        WordCount = c.WordCount,
                        PublishDate = c.PublishDate,
                        IsPremium = c.IsPremium,
                        UnlockPrice = c.UnlockPrice
                    })
                    .ToList();

                var response = new
                {
                    Id = novel.Id,
                    Title = novel.Title,
                    AlternativeTitle = novel.AlternativeTitle,
                    Synopsis = novel.Synopsis,
                    Status = novel.Status,
                    AverageRating = novel.AverageRating,
                    TotalRatings = novel.TotalRatings,
                    TotalChapters = novel.TotalChapters,
                    ViewCount = novel.ViewCount,
                    BookmarkCount = novel.BookmarkCount,
                    WordCount = novel.WordCount,
                    IsPremium = novel.IsPremium,
                    PublishDate = novel.PublishDate,
                    LastUpdated = novel.LastUpdated,
                    Language = novel.Language,
                    Author = new
                    {
                        Id = author?.Id ?? 0,
                        PenName = author?.PenName ?? "Unknown Author",
                        IsVerified = author?.IsVerified ?? false
                    },
                    Genres = genres.Select(g => new
                    {
                        Id = g.Id,
                        Name = g.Name,
                        ColorCode = g.ColorCode
                    }).ToList(),
                    RecentChapters = recentChapters
                };

                // Increment view count
                novel.ViewCount++;
                db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving novel: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/5/chapters - Get chapters list for a novel
        [HttpGet]
        [Route("{novelId:int}/chapters")]
        public IHttpActionResult GetNovelChapters(int novelId, int page = 1, int pageSize = 50)
        {
            try
            {
                // Verify novel exists
                var novel = db.Novels.FirstOrDefault(n => n.Id == novelId && n.IsActive);
                if (novel == null)
                {
                    return NotFound();
                }

                var query = db.Chapters
                    .Where(c => c.NovelId == novelId && c.IsPublished)
                    .OrderBy(c => c.ChapterNumber);

                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var chapters = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ChapterSummaryDto
                    {
                        Id = c.Id,
                        ChapterNumber = c.ChapterNumber,
                        Title = c.Title,
                        WordCount = c.WordCount,
                        PublishDate = c.PublishDate,
                        IsPremium = c.IsPremium,
                        UnlockPrice = c.UnlockPrice
                    })
                    .ToList();

                return Ok(new PaginatedApiResponse<List<ChapterSummaryDto>>
                {
                    Success = true,
                    Data = chapters,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving chapters: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/featured - Get featured novels
        [HttpGet]
        [Route("featured")]
        public IHttpActionResult GetFeaturedNovels()
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsFeatured);

                // Try to add ModerationStatus filter if it exists
                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var featuredNovelsList = query
                    .OrderByDescending(n => n.ViewCount)
                    .Take(10)
                    .ToList();

                // Get additional data separately
                var novelIds = featuredNovelsList.Select(n => n.Id).ToList();
                var authorIds = featuredNovelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName);

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var featuredNovels = featuredNovelsList.Select(n => new NovelSummaryDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 150 ?
                              n.Synopsis.Substring(0, 150) + "..." : n.Synopsis ?? "",
                    Status = n.Status,
                    AverageRating = n.AverageRating,
                    TotalChapters = n.TotalChapters,
                    ViewCount = n.ViewCount,
                    IsPremium = n.IsPremium,
                    LastUpdated = n.LastUpdated,
                    Genres = novelGenres.ContainsKey(n.Id) ? novelGenres[n.Id] : new List<string>()
                }).ToList();

                return Ok(new ApiResponse<List<NovelSummaryDto>>
                {
                    Success = true,
                    Data = featuredNovels
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving featured novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
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