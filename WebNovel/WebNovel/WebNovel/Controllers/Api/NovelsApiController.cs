using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ApiModels;

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
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var allNovels = query.ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    allNovels = allNovels.Where(n =>
                        n.Title != null && n.Title.ToLower().Contains(search.ToLower()) ||
                        (n.Synopsis != null && n.Synopsis.ToLower().Contains(search.ToLower()))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    allNovels = allNovels.Where(n =>
                        n.Status != null && n.Status.ToLower() == status.ToLower()
                    ).ToList();
                }

                if (genreId.HasValue)
                {
                    var novelsWithGenre = db.NovelGenres
                        .Where(ng => ng.GenreId == genreId.Value)
                        .Select(ng => ng.NovelId)
                        .ToList();

                    allNovels = allNovels.Where(n => novelsWithGenre.Contains(n.Id)).ToList();
                }

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

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

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

        // GET: api/novels/featured
        [HttpGet]
        [Route("featured")]
        public IHttpActionResult GetFeaturedNovels()
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsFeatured);

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var featuredNovelsList = query
                    .OrderByDescending(n => n.ViewCount)
                    .Take(10)
                    .ToList();

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

        // GET: api/novels/5
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

                var author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);

                var genres = db.NovelGenres
                    .Where(ng => ng.NovelId == id)
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id,
                          (ng, g) => new { Id = g.Id, Name = g.Name, ColorCode = g.ColorCode })
                    .ToList();

                // Get tags for the novel
                var tags = db.NovelTags
                    .Where(nt => nt.NovelId == id)
                    .Join(db.Tags, nt => nt.TagId, t => t.Id,
                          (nt, t) => new { Id = t.Id, Name = t.Name })
                    .ToList();

                var recentChapters = db.Chapters
                    .Where(c => c.NovelId == id && c.IsPublished)
                    .OrderByDescending(c => c.ChapterNumber)
                    .Take(12)
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
                    Tags = tags.Select(t => new
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList(),
                    RecentChapters = recentChapters
                };

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

        // GET: api/novels/slider-featured
        [HttpGet]
        [Route("slider-featured")]
        public IHttpActionResult GetSliderFeaturedNovels(int count = 10)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsSliderFeatured);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var novels = query
                    .OrderByDescending(n => n.LastUpdated)
                    .Take(count)
                    .ToList();

                var novelIds = novels.Select(n => n.Id).ToList();
                var authorIds = novels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = novels.Select(n => new NovelSummaryDto
                {
                    Id = n.Id,
                    Title = n.Title ?? "Untitled",
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 150 ?
                              n.Synopsis.Substring(0, 150) + "..." : n.Synopsis ?? "",
                    Status = n.Status ?? "Unknown",
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
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving slider featured novels: " + ex.Message));
            }
        }

        // GET: api/novels/weekly-featured
        [HttpGet]
        [Route("weekly-featured")]
        public IHttpActionResult GetWeeklyFeaturedNovels(int count = 6)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsWeeklyFeatured);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var novels = query
                    .OrderByDescending(n => n.LastUpdated)
                    .Take(count)
                    .ToList();

                var novelIds = novels.Select(n => n.Id).ToList();
                var authorIds = novels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = novels.Select(n => new NovelSummaryDto
                {
                    Id = n.Id,
                    Title = n.Title ?? "Untitled",
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 150 ?
                              n.Synopsis.Substring(0, 150) + "..." : n.Synopsis ?? "",
                    Status = n.Status ?? "Unknown",
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
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving weekly featured novels: " + ex.Message));
            }
        }

        // Add these two methods to your NovelsApiController class

        // GET: api/novels/premium
        [HttpGet]
        [Route("premium")]
        public IHttpActionResult GetPremiumNovels(int page = 1, int pageSize = 20, string sortBy = "updated")
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsPremium);

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var allNovels = query.ToList();

                // Apply sorting
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

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

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
                return InternalServerError(new Exception("Error retrieving premium novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/featured-list
        [HttpGet]
        [Route("featured-list")]
        public IHttpActionResult GetFeaturedNovelsList(int page = 1, int pageSize = 20, string sortBy = "updated")
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.IsFeatured);

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var allNovels = query.ToList();

                // Apply sorting
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

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

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
                return InternalServerError(new Exception("Error retrieving featured novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/newly-released
        [HttpGet]
        [Route("newly-released")]
        public IHttpActionResult GetNewlyReleasedNovels(int count = 20)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var novels = query
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(count)
                    .ToList();

                var novelIds = novels.Select(n => n.Id).ToList();
                var authorIds = novels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = novels.Select(n => new NovelSummaryDto
                {
                    Id = n.Id,
                    Title = n.Title ?? "Untitled",
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 150 ?
                              n.Synopsis.Substring(0, 150) + "..." : n.Synopsis ?? "",
                    Status = n.Status ?? "Unknown",
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
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving newly released novels: " + ex.Message));
            }
        }
        // GET: api/novels/5/cover
        [HttpGet]
        [Route("{id:int}/cover")]
        public IHttpActionResult GetNovelCover(int id)
        {
            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == id && n.IsActive);

                if (novel == null || novel.CoverImage == null || novel.CoverImage.Length == 0)
                {
                    return NotFound();
                }

                return new ImageResult(novel.CoverImage, "image/jpeg");
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving cover image: " + ex.Message));
            }
        }

        // GET: api/novels/5/chapters - Get all chapters for a novel
        [HttpGet]
        [Route("{novelId:int}/chapters")]
        public IHttpActionResult GetNovelChapters(int novelId, int page = 1, int pageSize = 50)
        {
            try
            {
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

        // GET: api/novels/ranking
        [HttpGet]
        [Route("ranking")]
        public IHttpActionResult GetNovelRanking(
            string type = "views",
            string period = "all",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                // Apply period filter if needed (you can extend this with a tracking table)
                DateTime? startDate = null;
                switch (period.ToLower())
                {
                    case "daily":
                        startDate = DateTime.Now.AddDays(-1);
                        break;
                    case "weekly":
                        startDate = DateTime.Now.AddDays(-7);
                        break;
                    case "monthly":
                        startDate = DateTime.Now.AddMonths(-1);
                        break;
                    case "yearly":
                        startDate = DateTime.Now.AddYears(-1);
                        break;
                    case "all":
                    default:
                        startDate = null;
                        break;
                }

                // For period filtering, you'd need a views tracking table
                // For now, we'll use LastUpdated as a proxy for recent activity
                if (startDate.HasValue)
                {
                    query = query.Where(n => n.LastUpdated >= startDate.Value);
                }

                var allNovels = query.ToList();

                // Apply ranking type
                switch (type.ToLower())
                {
                    case "views":
                        allNovels = allNovels.OrderByDescending(n => n.ViewCount).ToList();
                        break;
                    case "bookmarks":
                        allNovels = allNovels.OrderByDescending(n => n.BookmarkCount).ToList();
                        break;
                    case "rating":
                        allNovels = allNovels.OrderByDescending(n => n.AverageRating)
                            .ThenByDescending(n => n.TotalRatings).ToList();
                        break;
                    case "chapters":
                        allNovels = allNovels.OrderByDescending(n => n.TotalChapters).ToList();
                        break;
                    case "words":
                        allNovels = allNovels.OrderByDescending(n => n.WordCount).ToList();
                        break;
                    default:
                        allNovels = allNovels.OrderByDescending(n => n.ViewCount).ToList();
                        break;
                }

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var startRank = (page - 1) * pageSize + 1;
                var novels = novelsList.Select((n, index) => new NovelRankingDto
                {
                    Rank = startRank + index,
                    Id = n.Id,
                    Title = n.Title ?? "Untitled",
                    AuthorName = authors.ContainsKey(n.AuthorId) ? authors[n.AuthorId] : "Unknown Author",
                    Synopsis = n.Synopsis != null && n.Synopsis.Length > 200 ?
                              n.Synopsis.Substring(0, 200) + "..." : n.Synopsis ?? "",
                    Status = n.Status ?? "Unknown",
                    AverageRating = n.AverageRating,
                    TotalRatings = n.TotalRatings,
                    TotalChapters = n.TotalChapters,
                    ViewCount = n.ViewCount,
                    BookmarkCount = n.BookmarkCount,
                    WordCount = n.WordCount,
                    IsPremium = n.IsPremium,
                    LastUpdated = n.LastUpdated,
                    Genres = novelGenres.ContainsKey(n.Id) ? novelGenres[n.Id] : new List<string>()
                }).ToList();

                return Ok(new PaginatedApiResponse<List<NovelRankingDto>>
                {
                    Success = true,
                    Data = novels,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    Message = $"Ranking by {type} for period: {period}"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving novel rankings: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/ranking/top - Get top novels across different categories
        [HttpGet]
        [Route("ranking/top")]
        public IHttpActionResult GetTopNovels(int count = 10)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var allNovels = query.ToList();

                var topByViews = allNovels
                    .OrderByDescending(n => n.ViewCount)
                    .Take(count)
                    .Select(n => CreateSimpleNovelDto(n))
                    .ToList();

                var topByRating = allNovels
                    .Where(n => n.TotalRatings >= 5) // Minimum ratings threshold
                    .OrderByDescending(n => n.AverageRating)
                    .ThenByDescending(n => n.TotalRatings)
                    .Take(count)
                    .Select(n => CreateSimpleNovelDto(n))
                    .ToList();

                var topByBookmarks = allNovels
                    .OrderByDescending(n => n.BookmarkCount)
                    .Take(count)
                    .Select(n => CreateSimpleNovelDto(n))
                    .ToList();

                var topByChapters = allNovels
                    .OrderByDescending(n => n.TotalChapters)
                    .Take(count)
                    .Select(n => CreateSimpleNovelDto(n))
                    .ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        MostViewed = topByViews,
                        HighestRated = topByRating,
                        MostBookmarked = topByBookmarks,
                        MostChapters = topByChapters
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving top novels: " + ex.Message));
            }
        }

        // Helper method to create simple novel DTO
        private object CreateSimpleNovelDto(Novel n)
        {
            var author = db.Authors.FirstOrDefault(a => a.Id == n.AuthorId);
            var genres = db.NovelGenres
                .Where(ng => ng.NovelId == n.Id)
                .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => g.Name)
                .ToList();

            return new
            {
                Id = n.Id,
                Title = n.Title ?? "Untitled",
                AuthorName = author?.PenName ?? "Unknown Author",
                Status = n.Status ?? "Unknown",
                AverageRating = n.AverageRating,
                TotalRatings = n.TotalRatings,
                ViewCount = n.ViewCount,
                BookmarkCount = n.BookmarkCount,
                TotalChapters = n.TotalChapters,
                IsPremium = n.IsPremium,
                Genres = genres
            };
        }

        // GET: api/novels/5/chapters/10 - Get chapter by chapter number
        [HttpGet]
        [Route("{novelId:int}/chapters/{chapterNumber:int}")]
        public IHttpActionResult GetChapter(int novelId, int chapterNumber, int? userId = null)
        {
            try
            {
                var chapter = db.Chapters
                    .Include(c => c.Novel)
                    .Include(c => c.Novel.Author)
                    .FirstOrDefault(c => c.ChapterNumber == chapterNumber && c.NovelId == novelId && c.IsPublished);

                if (chapter == null)
                {
                    return Content(HttpStatusCode.NotFound, new
                    {
                        Success = false,
                        Message = $"Chapter {chapterNumber} not found for novel {novelId}"
                    });
                }

                bool hasAccess = true;
                string accessReason = "free";

                if (chapter.IsPremium || chapter.UnlockPrice > 0)
                {
                    if (userId.HasValue)
                    {
                        var unlocked = db.UnlockedChapters
                            .Any(uc => uc.UserId == userId.Value && uc.ChapterId == chapter.Id);

                        if (!unlocked)
                        {
                            var reader = db.Readers.FirstOrDefault(r => r.UserId == userId.Value);

                            if (reader?.IsPremium == true && reader.PremiumExpiryDate > DateTime.Now)
                            {
                                accessReason = "premium";
                            }
                            else
                            {
                                hasAccess = false;
                                accessReason = "locked";
                            }
                        }
                        else
                        {
                            accessReason = "purchased";
                        }
                    }
                    else
                    {
                        hasAccess = false;
                        accessReason = "login_required";
                    }
                }

                var response = new ChapterDetailDto
                {
                    Id = chapter.Id,
                    NovelId = chapter.NovelId,
                    ChapterNumber = chapter.ChapterNumber,
                    Title = chapter.Title,
                    Content = hasAccess ? chapter.Content : null,
                    WordCount = chapter.WordCount,
                    PublishDate = chapter.PublishDate,
                    IsPremium = chapter.IsPremium,
                    UnlockPrice = chapter.UnlockPrice,
                    PreviewContent = hasAccess ? null : GetPreviewContent(chapter)
                };

                if (hasAccess)
                {
                    chapter.ViewCount++;
                    db.SaveChanges();
                }

                return Ok(new
                {
                    Success = true,
                    Data = response,
                    AccessInfo = new
                    {
                        HasAccess = hasAccess,
                        AccessReason = accessReason,
                        RequiredCoins = chapter.UnlockPrice,
                        IsPremium = chapter.IsPremium
                    },
                    Novel = new
                    {
                        Id = chapter.Novel.Id,
                        Title = chapter.Novel.Title,
                        AuthorName = chapter.Novel.Author?.PenName ?? "Unknown Author"
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving chapter: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // POST: api/novels/5/chapters/10/unlock - Unlock chapter by chapter number
        [HttpPost]
        [Route("{novelId:int}/chapters/{chapterNumber:int}/unlock")]
        public IHttpActionResult UnlockChapter(int novelId, int chapterNumber, [FromBody] UnlockChapterRequest request)
        {
            if (request?.UserId == null)
            {
                return BadRequest("User ID is required");
            }

            try
            {
                var chapter = db.Chapters.FirstOrDefault(c => c.ChapterNumber == chapterNumber && c.NovelId == novelId);
                if (chapter == null)
                {
                    return NotFound();
                }

                if (chapter.UnlockPrice == 0)
                {
                    return BadRequest("This chapter is already free");
                }

                var existing = db.UnlockedChapters
                    .FirstOrDefault(uc => uc.UserId == request.UserId && uc.ChapterId == chapter.Id);

                if (existing != null)
                {
                    return BadRequest("Chapter already unlocked");
                }

                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == request.UserId);
                if (wallet == null || wallet.CoinBalance < chapter.UnlockPrice)
                {
                    return BadRequest("Insufficient coins");
                }

                decimal unlockPrice = Convert.ToDecimal(chapter.UnlockPrice);

                wallet.CoinBalance -= unlockPrice;
                wallet.TotalCoinsSpent += unlockPrice;
                wallet.LastUpdated = DateTime.Now;
                wallet.UpdatedAt = DateTime.Now;

                var transaction = new CoinTransaction
                {
                    UserId = request.UserId,
                    TransactionType = "Spend",
                    Amount = -Convert.ToInt32(unlockPrice),
                    BalanceBefore = Convert.ToInt32(wallet.CoinBalance + unlockPrice),
                    BalanceAfter = Convert.ToInt32(wallet.CoinBalance),
                    RelatedChapterId = chapter.Id,
                    Description = $"Unlocked Chapter {chapter.ChapterNumber}: {chapter.Title}"
                };
                db.CoinTransactions.Add(transaction);

                var unlock = new UnlockedChapter
                {
                    UserId = request.UserId,
                    ChapterId = chapter.Id,
                    UnlockMethod = "Coins",
                    CoinsSpent = Convert.ToInt32(unlockPrice)
                };
                db.UnlockedChapters.Add(unlock);

                db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Chapter unlocked successfully",
                    Data = new
                    {
                        NewCoinBalance = wallet.CoinBalance,
                        CoinsSpent = unlockPrice
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error unlocking chapter: " + ex.Message));
            }
        }

        // GET: api/novels/surprise-me
        [HttpGet]
        [Route("surprise-me")]
        public IHttpActionResult GetSurpriseNovel(int? userId = null)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var allNovels = query.ToList();

                if (!allNovels.Any())
                {
                    return NotFound();
                }

                Random rnd = new Random();
                var randomNovel = allNovels[rnd.Next(allNovels.Count)];

                var author = db.Authors.FirstOrDefault(a => a.Id == randomNovel.AuthorId);

                var genres = db.NovelGenres
                    .Where(ng => ng.NovelId == randomNovel.Id)
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => g.Name)
                    .ToList();

                var result = new
                {
                    Id = randomNovel.Id,
                    Title = randomNovel.Title ?? "Untitled",
                    AuthorName = author?.PenName ?? "Unknown Author",
                    Synopsis = randomNovel.Synopsis ?? "",
                    Status = randomNovel.Status ?? "Unknown",
                    AverageRating = randomNovel.AverageRating,
                    TotalChapters = randomNovel.TotalChapters,
                    ViewCount = randomNovel.ViewCount,
                    IsPremium = randomNovel.IsPremium,
                    LastUpdated = randomNovel.LastUpdated,
                    Genres = genres
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Message = "Here's a surprise novel for you!"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving surprise novel: " + ex.Message));
            }
        }

        // GET: api/novels/random - Paginated Simple Random
        [HttpGet]
        [Route("random")]
        public IHttpActionResult GetRandomNovels(int page = 1, int pageSize = 30, int? userId = null)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var allNovels = query.ToList();
                int seed;
                Random rnd;
                if (userId.HasValue)
                {
                    seed = userId.Value + DateTime.Now.Date.GetHashCode();
                    rnd = new Random(seed);
                }
                else
                {
                    seed = DateTime.Now.Date.GetHashCode();
                    rnd = new Random(seed);
                }
                var shuffled = allNovels.OrderBy(x => rnd.Next()).ToList();

                var totalCount = shuffled.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var paginatedNovels = shuffled
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = paginatedNovels.Select(n => n.Id).ToList();
                var authorIds = paginatedNovels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = paginatedNovels.Select(n => new NovelSummaryDto
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
                    Data = result,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    Message = "Randomized novel recommendations"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving random novels: " + ex.Message));
            }
        }

        // GET: api/novels/discover - Paginated Discover
        [HttpGet]
        [Route("discover")]
        public IHttpActionResult GetDiscoverNovels(int page = 1, int pageSize = 30, int? userId = null, string preference = "balanced")
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var allNovels = query.ToList();

                Random rnd;
                if (userId.HasValue)
                {
                    int seed = userId.Value + DateTime.Now.Date.GetHashCode();
                    rnd = new Random(seed);
                }
                else
                {
                    int seed = DateTime.Now.Date.GetHashCode();
                    rnd = new Random(seed);
                }

                List<Novel> orderedNovels;

                // Order ALL novels based on preference (not just first page)
                switch (preference.ToLower())
                {
                    case "popular":
                        var popular = allNovels
                            .OrderByDescending(n => n.ViewCount)
                            .Take((int)(allNovels.Count * 0.7))
                            .ToList();
                        var randomPop = allNovels
                            .Except(popular)
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        orderedNovels = popular.Concat(randomPop)
                            .Distinct() // Add Distinct to remove duplicates
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        break;

                    case "new":
                        var newNovels = allNovels
                            .OrderByDescending(n => n.CreatedAt)
                            .Take((int)(allNovels.Count * 0.7))
                            .ToList();
                        var randomNew = allNovels
                            .Except(newNovels)
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        orderedNovels = newNovels.Concat(randomNew)
                            .Distinct() // Add Distinct to remove duplicates
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        break;

                    case "highrated":
                        var highRated = allNovels
                            .Where(n => n.TotalRatings >= 5)
                            .OrderByDescending(n => n.AverageRating)
                            .Take((int)(allNovels.Count * 0.7))
                            .ToList();
                        var randomRated = allNovels
                            .Except(highRated)
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        orderedNovels = highRated.Concat(randomRated)
                            .Distinct() // Add Distinct to remove duplicates
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        break;

                    case "balanced":
                    default:
                        var top = allNovels.OrderByDescending(n => n.ViewCount).Take(allNovels.Count / 3).ToList();
                        var recent = allNovels.OrderByDescending(n => n.LastUpdated).Take(allNovels.Count / 3).ToList();
                        var random = allNovels
                            .Except(top.Union(recent)) // Use Union instead of Concat to avoid duplicates in the source
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        orderedNovels = top.Union(recent).Union(random) // Use Union instead of Concat
                            .OrderBy(x => rnd.Next())
                            .ToList();
                        break;
                }

                // Apply pagination
                var totalCount = orderedNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var paginatedNovels = orderedNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = paginatedNovels.Select(n => n.Id).ToList();
                var authorIds = paginatedNovels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = paginatedNovels.Select(n => new NovelSummaryDto
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
                    Data = result,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    Message = $"Discover novels with '{preference}' preference"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving discover novels: " + ex.Message));
            }
        }

        // GET: api/novels/weighted-random
        [HttpGet]
        [Route("weighted-random")]
        public IHttpActionResult GetWeightedRandomNovels(int count = 20, int? userId = null)
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive);

                try
                {
                    query = query.Where(n => n.ModerationStatus == "Approved");
                }
                catch { }

                var novelsForWeighting = query.Select(n => new
                {
                    NovelObject = n,
                    n.ViewCount,
                    n.AverageRating,
                    n.TotalRatings,
                    n.BookmarkCount,
                    n.LastUpdated
                }).ToList();

                Random rnd;
                if (userId.HasValue)
                {
                    int seed = userId.Value + DateTime.Now.Date.GetHashCode();
                    rnd = new Random(seed);
                }
                else
                {
                    rnd = new Random();
                }

                var novelsWithWeights = novelsForWeighting.Select(n => new
                {
                    Novel = n.NovelObject,
                    Weight = CalculateNovelWeight(n.ViewCount, n.AverageRating, n.TotalRatings, n.BookmarkCount, n.LastUpdated)
                }).ToList();

                var selectedNovels = new List<Novel>();
                var availableNovels = novelsWithWeights
                    .Select(nw => (Novel: nw.Novel, Weight: nw.Weight))
                    .ToList();

                double totalWeight = availableNovels.Sum(n => n.Weight);

                for (int i = 0; i < count && availableNovels.Any(); i++)
                {
                    double randomValue = rnd.NextDouble() * totalWeight;
                    double cumulative = 0;
                    int selectedIndex = -1;

                    for (int j = 0; j < availableNovels.Count; j++)
                    {
                        cumulative += availableNovels[j].Weight;
                        if (randomValue <= cumulative)
                        {
                            selectedIndex = j;
                            break;
                        }
                    }

                    if (selectedIndex != -1)
                    {
                        var selectedItem = availableNovels[selectedIndex];
                        selectedNovels.Add(selectedItem.Novel);

                        totalWeight -= selectedItem.Weight;

                        availableNovels[selectedIndex] = availableNovels[availableNovels.Count - 1];
                        availableNovels.RemoveAt(availableNovels.Count - 1);
                    }
                }

                var novelIds = selectedNovels.Select(n => n.Id).ToList();
                var authorIds = selectedNovels.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                var result = selectedNovels.Select(n => new NovelSummaryDto
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

                return Ok(new ApiResponse<List<NovelSummaryDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Weighted random recommendations - popular novels appear more frequently"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error retrieving weighted random novels: " + ex.Message));
            }
        }

        private double CalculateNovelWeight(long viewCount, decimal averageRating, int totalRatings, long bookmarkCount, DateTime lastUpdated)
        {
            double viewWeight = Math.Log10(viewCount + 1) * 2;
            double ratingWeight = (double)averageRating * totalRatings / 10.0;
            double bookmarkWeight = Math.Log10(bookmarkCount + 1) * 1.5;

            var daysSinceUpdate = (DateTime.Now - lastUpdated).TotalDays;
            double recencyBoost = daysSinceUpdate < 7 ? 1.5 : (daysSinceUpdate < 30 ? 1.2 : 1.0);

            double baseWeight = viewWeight + ratingWeight + bookmarkWeight;
            return Math.Max(baseWeight * recencyBoost, 1.0);
        }

        // GET: api/novels/ongoing - Get all ongoing novels
        [HttpGet]
        [Route("ongoing")]
        public IHttpActionResult GetOngoingNovels(int page = 1, int pageSize = 20, string sortBy = "updated")
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.Status == "Ongoing");

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var allNovels = query.ToList();

                // Apply sorting
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

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

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
                return InternalServerError(new Exception("Error retrieving ongoing novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        // GET: api/novels/completed - Get all completed novels
        [HttpGet]
        [Route("completed")]
        public IHttpActionResult GetCompletedNovels(int page = 1, int pageSize = 20, string sortBy = "updated")
        {
            try
            {
                var query = db.Novels.Where(n => n.IsActive && n.Status == "Completed");

                try
                {
                    var testQuery = query.Where(n => n.ModerationStatus == "Approved").Take(1).ToList();
                    query = query.Where(n => string.IsNullOrEmpty(n.ModerationStatus) || n.ModerationStatus == "Approved");
                }
                catch
                {
                    // Skip ModerationStatus filter if it doesn't work
                }

                var allNovels = query.ToList();

                // Apply sorting
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

                var totalCount = allNovels.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var novelsList = allNovels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var novelIds = novelsList.Select(n => n.Id).ToList();
                var authorIds = novelsList.Select(n => n.AuthorId).Distinct().ToList();

                var authors = db.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a.PenName ?? "Unknown Author");

                var novelGenres = db.NovelGenres
                    .Where(ng => novelIds.Contains(ng.NovelId))
                    .Join(db.Genres, ng => ng.GenreId, g => g.Id, (ng, g) => new { ng.NovelId, g.Name })
                    .GroupBy(x => x.NovelId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

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
                return InternalServerError(new Exception("Error retrieving completed novels: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")));
            }
        }

        private string GetPreviewContent(Chapter chapter)
        {
            if (string.IsNullOrEmpty(chapter.Content))
                return "";

            var previewLength = Math.Min(200, chapter.Content.Length);
            return chapter.Content.Substring(0, previewLength) + "...";
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

    public class UnlockChapterRequest
    {
        public int UserId { get; set; }
    }
}