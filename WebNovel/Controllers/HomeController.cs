using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Controllers
{
    public class HomeController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        #region Main Actions

        public ActionResult Index()
        {
            try
            {
                var sliderNovels = GetSliderNovels(10);
                var weeklyFeaturedNovels = GetWeeklyFeaturedNovels(6);
                var newlyReleasedNovels = GetNewlyReleasedNovels(20);

                // Get all approved novels for rankings
                var allNovels = db.Novels
                    .Where(n => n.IsActive && n.ModerationStatus == "Approved")
                    .ToList();

                // Get popular genres and default genre novels
                var popularGenres = GetPopularGenres(8);
                var defaultGenreNovels = new List<Novel>();
                var defaultGenre = popularGenres.FirstOrDefault();

                if (defaultGenre != null)
                {
                    defaultGenreNovels = GetNovelsByGenreId(defaultGenre.Id, 18);
                }

                ViewBag.WeeklyFeaturedNovels = weeklyFeaturedNovels;        //For Weekly Featured novel
                ViewBag.NewlyReleasedNovels = newlyReleasedNovels;          //For Newly releases slider
                ViewBag.AllNovels = allNovels;                              //For Ranking
                ViewBag.PopularGenres = popularGenres;                      //For Popular Genres tabs
                ViewBag.DefaultGenreNovels = defaultGenreNovels;            //For Popular Genres default novels
                ViewBag.DefaultGenre = defaultGenre;                        //For Popular Genres default genre

                // Return slider novels as before to maintain slider functionality
                return View(sliderNovels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Index: {ex.Message}");
                ViewBag.WeeklyFeaturedNovels = new List<Novel>();
                ViewBag.NewlyReleasedNovels = new List<Novel>();
                ViewBag.AllNovels = new List<Novel>();
                ViewBag.PopularGenres = new List<Genre>();
                ViewBag.DefaultGenreNovels = new List<Novel>();
                ViewBag.DefaultGenre = null;
                return View(new List<Novel>());
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        #endregion

        #region Genre Navigation

        #region Popular Genres

        private List<Genre> GetPopularGenres(int count = 8)
        {
            try
            {
                // Get genres ordered by the number of novels they have
                var popularGenres = db.Genres
                    .Where(g => g.IsActive)
                    .Select(g => new {
                        Genre = g,
                        NovelCount = db.NovelGenres.Count(ng => ng.GenreId == g.Id && ng.Novel.IsActive && ng.Novel.ModerationStatus == "Approved")
                    })
                    .Where(x => x.NovelCount > 0)
                    .OrderByDescending(x => x.NovelCount)
                    .Take(count)
                    .Select(x => x.Genre)
                    .ToList();

                return popularGenres;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting popular genres: {ex.Message}");
                return new List<Genre>();
            }
        }

        private List<Novel> GetNovelsByGenreId(int genreId, int count = 18)
        {
            try
            {
                var novels = db.NovelGenres
                    .Where(ng => ng.GenreId == genreId)
                    .Select(ng => ng.Novel)
                    .Where(n => n.IsActive && n.ModerationStatus == "Approved")
                    .OrderBy(n => Guid.NewGuid()) // This randomizes the order
                    .Take(count)
                    .ToList();

                // Load genres for each novel
                foreach (var novel in novels)
                {
                    try
                    {
                        var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                        novel.Genres = new List<Genre>();

                        foreach (var ng in novelGenres)
                        {
                            var genre = db.Genres.Find(ng.GenreId);
                            if (genre != null)
                            {
                                novel.Genres.Add(genre);
                            }
                        }
                    }
                    catch
                    {
                        novel.Genres = new List<Genre>();
                    }
                }

                return novels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting novels by genre: {ex.Message}");
                return new List<Novel>();
            }
        }

        [HttpGet]
        public JsonResult GetNovelsByGenre(int genreId, int count = 18)
        {
            try
            {
                // Get the genre
                var genre = db.Genres.Find(genreId);
                if (genre == null)
                {
                    return Json(new { success = false, message = "Genre not found" }, JsonRequestBehavior.AllowGet);
                }

                // Get novels for this genre
                var novels = GetNovelsByGenreId(genreId, count);

                var result = novels.Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    status = n.Status,
                    averageRating = Math.Round(n.AverageRating, 1),
                    totalChapters = n.TotalChapters,
                    bookmarkCount = n.BookmarkCount,
                    hasImage = n.HasCoverImage,
                    coverImageUrl = n.HasCoverImage ? Url.Action("GetCoverImage", "Home", new { id = n.Id }) : null
                }).ToList();

                return Json(new
                {
                    success = true,
                    genre = new
                    {
                        id = genre.Id,
                        name = genre.Name,
                        colorCode = genre.ColorCode ?? "#77DD77"
                    },
                    novels = result,
                    count = result.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #endregion

        #region Newly Releases
        private List<Novel> GetNewlyReleasedNovels(int count = 20)
        {
            try
            {
                var newlyReleasedNovels = db.Novels
                    .Where(n => n.IsActive &&
                               n.ModerationStatus == "Approved")
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(count)
                    .ToList();

                // Load genres for each novel
                foreach (var novel in newlyReleasedNovels)
                {
                    try
                    {
                        var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                        novel.Genres = new List<Genre>();

                        foreach (var ng in novelGenres)
                        {
                            var genre = db.Genres.Find(ng.GenreId);
                            if (genre != null)
                            {
                                novel.Genres.Add(genre);
                            }
                        }
                    }
                    catch
                    {
                        novel.Genres = new List<Genre>();
                    }
                }

                return newlyReleasedNovels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting newly released novels: {ex.Message}");
                return new List<Novel>();
            }
        }
        #endregion

        #region Weekly Featured
        private List<Novel> GetWeeklyFeaturedNovels(int count = 6)
        {
            try
            {
                var weeklyFeaturedNovels = db.Novels
                    .Where(n => n.IsWeeklyFeatured == true &&
                               n.IsActive &&
                               n.ModerationStatus == "Approved")
                    .OrderByDescending(n => n.LastUpdated)
                    .Take(count)
                    .ToList();

                // Load genres for each novel
                foreach (var novel in weeklyFeaturedNovels)
                {
                    try
                    {
                        var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                        novel.Genres = new List<Genre>();

                        foreach (var ng in novelGenres)
                        {
                            var genre = db.Genres.Find(ng.GenreId);
                            if (genre != null)
                            {
                                novel.Genres.Add(genre);
                            }
                        }
                    }
                    catch
                    {
                        novel.Genres = new List<Genre>();
                    }
                }

                return weeklyFeaturedNovels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting weekly featured novels: {ex.Message}");
                return new List<Novel>();
            }
        }
        #endregion

        #region Slider Methods



        private List<Novel> GetSliderNovels(int count = 10)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== GETTING SLIDER NOVELS ===");

                // Load all novels without navigation properties to avoid EF issues
                var allNovels = db.Novels.ToList();
                System.Diagnostics.Debug.WriteLine($"Total novels loaded: {allNovels.Count}");

                // Get slider featured novels first
                var sliderFeaturedNovels = allNovels
                    .Where(n => n.IsSliderFeatured == true &&
                               n.IsActive &&
                               n.ModerationStatus == "Approved")
                    .OrderByDescending(n => n.LastUpdated)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Slider featured novels found: {sliderFeaturedNovels.Count}");

                var result = new List<Novel>(sliderFeaturedNovels);

                // Fill remaining slots with high-quality novels
                if (result.Count < count)
                {
                    var remainingCount = count - result.Count;
                    var existingIds = result.Select(n => n.Id).ToHashSet();

                    var additionalNovels = allNovels
                        .Where(n => n.IsActive &&
                                   n.ModerationStatus == "Approved" &&
                                   !existingIds.Contains(n.Id))
                        .OrderByDescending(n => n.AverageRating)
                        .ThenByDescending(n => n.ViewCount)
                        .Take(remainingCount)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Additional novels added: {additionalNovels.Count}");
                    result.AddRange(additionalNovels);
                }

                var finalResult = result.Take(count).ToList();

                // DEBUG: Log each novel and its author info
                System.Diagnostics.Debug.WriteLine("Final slider novels:");
                foreach (var novel in finalResult)
                {
                    System.Diagnostics.Debug.WriteLine($"- Novel: ID={novel.Id}, Title={novel.Title}, AuthorId={novel.AuthorId}");
                }

                // **FIX: Load author information for each novel**
                foreach (var novel in finalResult)
                {
                    try
                    {
                        // Load the author using the fixed method
                        var author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);
                        if (author != null)
                        {
                            novel.Author = author;
                            System.Diagnostics.Debug.WriteLine($"Loaded author for {novel.Title}: {author.PenName} (ID: {author.Id})");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No author found for {novel.Title} with AuthorId: {novel.AuthorId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading author for {novel.Title}: {ex.Message}");
                    }

                    // Also load genres (keep existing logic)
                    try
                    {
                        var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                        novel.NovelGenres = new List<NovelGenre>();

                        foreach (var ng in novelGenres)
                        {
                            var genre = db.Genres.Find(ng.GenreId);
                            if (genre != null)
                            {
                                ng.Genre = genre;
                                novel.NovelGenres.Add(ng);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading genres for {novel.Title}: {ex.Message}");
                        novel.NovelGenres = new List<NovelGenre>();
                    }
                }

                System.Diagnostics.Debug.WriteLine("===============================");
                return finalResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting slider novels: {ex.Message}");
                return new List<Novel>();
            }
        }

        /// <summary>
        /// API endpoint for AJAX refresh of slider data
        /// </summary>
        [HttpGet]
        public JsonResult RefreshSlider()
        {
            try
            {
                var novels = GetSliderNovels(10);

                var result = novels.Select(n => {
                    // DEBUG: Log novel details
                    System.Diagnostics.Debug.WriteLine($"Processing Novel: ID={n.Id}, Title={n.Title}, AuthorId={n.AuthorId}");

                    // Safely load author information - ALTERNATIVE VERSION
                    string authorName = "Unknown Author";
                    int? authorId = null;

                    try
                    {
                        if (n.AuthorId > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Looking up author with ID: {n.AuthorId} for novel: {n.Title}");

                            // Use a completely fresh context to avoid any EF caching issues
                            using (var freshDb = new DarkNovelDbContext())
                            {
                                var author = freshDb.Authors
                                    .Where(a => a.Id == n.AuthorId)
                                    .Select(a => new { a.Id, a.PenName })
                                    .FirstOrDefault();

                                if (author != null)
                                {
                                    authorName = author.PenName ?? "Unknown Author";
                                    authorId = author.Id;
                                    System.Diagnostics.Debug.WriteLine($"SUCCESS: Found {author.PenName} (ID: {author.Id}) for novel: {n.Title}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"FAILED: No author found with ID {n.AuthorId} for novel: {n.Title}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading author for novel {n.Id} ({n.Title}): {ex.Message}");
                    }

                    // DEBUG: Log final author assignment
                    System.Diagnostics.Debug.WriteLine($"Final assignment - Novel: {n.Title}, Author: {authorName}, AuthorId: {authorId}");
                    System.Diagnostics.Debug.WriteLine("---");

                    // Safely load genres
                    var genres = new List<object>();
                    try
                    {
                        var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == n.Id).ToList();
                        foreach (var ng in novelGenres)
                        {
                            var genre = db.Genres.Find(ng.GenreId);
                            if (genre != null)
                            {
                                genres.Add(new
                                {
                                    id = genre.Id,
                                    name = genre.Name,
                                    colorCode = genre.ColorCode ?? "#007bff",
                                    iconClass = genre.IconClass ?? "fas fa-book"
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Continue with empty genres if loading fails
                    }

                    return new
                    {
                        id = n.Id,
                        title = n.Title,
                        alternativeTitle = n.AlternativeTitle,
                        synopsis = !string.IsNullOrEmpty(n.Synopsis)
                            ? (n.Synopsis.Length > 150 ? n.Synopsis.Substring(0, 150) + "..." : n.Synopsis)
                            : "No description available...",
                        authorName = authorName,
                        authorId = authorId,
                        originalAuthorId = n.AuthorId, // DEBUG: Include original AuthorId for comparison
                        averageRating = Math.Round(n.AverageRating, 1),
                        totalChapters = n.TotalChapters,
                        totalRatings = n.TotalRatings,
                        viewCount = n.ViewCount,
                        bookmarkCount = n.BookmarkCount,
                        status = n.Status,
                        isPremium = n.IsPremium,
                        publishDate = n.PublishDate.ToString("yyyy-MM-dd"),
                        lastUpdated = n.LastUpdated.ToString("yyyy-MM-dd"),
                        genres = genres,
                        hasImage = n.HasCoverImage,
                        coverImageUrl = n.HasCoverImage ? Url.Action("GetCoverImage", "Home", new { id = n.Id }) : null
                    };
                }).ToList();

                // DEBUG: Final summary
                System.Diagnostics.Debug.WriteLine("=== SLIDER REFRESH SUMMARY ===");
                foreach (var item in result)
                {
                    System.Diagnostics.Debug.WriteLine($"Novel: {item.title}, AuthorName: {item.authorName}, OriginalAuthorId: {item.originalAuthorId}, ResolvedAuthorId: {item.authorId}");
                }
                System.Diagnostics.Debug.WriteLine("==============================");

                return Json(new
                {
                    success = true,
                    novels = result,
                    count = result.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in RefreshSlider: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// New debug method to check author-novel relationships
        /// </summary>
        [HttpGet]
        public JsonResult DebugAuthorNovelRelationships()
        {
            try
            {
                // Get all novels with their authors
                var novelAuthorData = db.Novels
                    .Where(n => n.IsActive && n.ModerationStatus == "Approved")
                    .Select(n => new
                    {
                        NovelId = n.Id,
                        NovelTitle = n.Title,
                        NovelAuthorId = n.AuthorId,
                        AuthorData = db.Authors.Where(a => a.Id == n.AuthorId)
                            .Select(a => new { a.Id, a.PenName }).FirstOrDefault()
                    })
                    .Take(20) // Limit for debugging
                    .ToList();

                var debugInfo = novelAuthorData.Select(item => new
                {
                    novelId = item.NovelId,
                    novelTitle = item.NovelTitle,
                    novelAuthorId = item.NovelAuthorId,
                    authorExists = item.AuthorData != null,
                    authorId = item.AuthorData?.Id,
                    authorPenName = item.AuthorData?.PenName,
                    idsMatch = item.NovelAuthorId == item.AuthorData?.Id
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = debugInfo,
                    summary = new
                    {
                        totalChecked = debugInfo.Count,
                        authorsFound = debugInfo.Count(d => d.authorExists),
                        missingAuthors = debugInfo.Count(d => !d.authorExists),
                        mismatchedIds = debugInfo.Count(d => d.authorExists && !d.idsMatch)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        // Add this simple debug method to your HomeController

        [HttpGet]
        public JsonResult DebugAuthorLookup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUGGING AUTHOR LOOKUP ===");

                // Check the specific novels from your slider
                var sliderNovelIds = new int[] { 11, 26, 23, 14, 13, 12, 5, 10, 9, 8 };

                foreach (var novelId in sliderNovelIds)
                {
                    var novel = db.Novels.Find(novelId);
                    if (novel != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Novel ID {novelId}: {novel.Title}");
                        System.Diagnostics.Debug.WriteLine($"  - Novel.AuthorId: {novel.AuthorId}");

                        // Try to find the author
                        var author = db.Authors.Find(novel.AuthorId);
                        if (author != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - Found Author: ID={author.Id}, Name={author.PenName}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  - NO AUTHOR FOUND for AuthorId {novel.AuthorId}");

                            // Check if any author exists with this ID using different method
                            var authorAlt = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);
                            if (authorAlt != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"  - BUT Found with FirstOrDefault: ID={authorAlt.Id}, Name={authorAlt.PenName}");
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("");
                    }
                }

                // Also show all authors to see what's available
                System.Diagnostics.Debug.WriteLine("=== ALL AUTHORS IN DATABASE ===");
                var allAuthors = db.Authors.ToList();
                foreach (var auth in allAuthors)
                {
                    System.Diagnostics.Debug.WriteLine($"Author ID={auth.Id}, Name={auth.PenName}");
                }

                return Json(new { success = true, message = "Check debug output" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region Novel Management

        /// <summary>
        /// Updates the slider featured status of a novel
        /// </summary>
        [HttpPost]
        public JsonResult UpdateSliderFeatured(int novelId, bool isSliderFeatured)
        {
            try
            {
                var novel = db.Novels.Find(novelId);
                if (novel == null)
                {
                    return Json(new { success = false, message = "Novel not found" });
                }

                novel.IsSliderFeatured = isSliderFeatured;
                novel.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Slider status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets detailed novel information for quick view/modal
        /// </summary>
        [HttpGet]
        public JsonResult GetNovelQuickView(int id)
        {
            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == id && n.IsActive);

                if (novel == null)
                {
                    return Json(new { success = false, message = "Novel not found" }, JsonRequestBehavior.AllowGet);
                }

                // Safely load related data
                string authorName = "Unknown Author";
                try
                {
                    var author = db.Authors.Find(novel.AuthorId);
                    authorName = author?.PenName ?? "Unknown Author";
                }
                catch { }

                var genres = new List<object>();
                try
                {
                    var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                    foreach (var ng in novelGenres)
                    {
                        var genre = db.Genres.Find(ng.GenreId);
                        if (genre != null)
                        {
                            genres.Add(new { name = genre.Name, colorCode = genre.ColorCode });
                        }
                    }
                }
                catch { }

                var latestChapters = new List<object>();
                try
                {
                    latestChapters = db.Chapters
                        .Where(c => c.NovelId == id)
                        .OrderByDescending(c => c.ChapterNumber)
                        .Take(3)
                        .Select(c => new {
                            id = c.Id,
                            title = c.Title,
                            chapterNumber = c.ChapterNumber,
                            publishDate = c.PublishDate.ToString("MMM dd")
                        })
                        .ToList<object>();
                }
                catch { }

                var result = new
                {
                    id = novel.Id,
                    title = novel.Title,
                    alternativeTitle = novel.AlternativeTitle,
                    synopsis = novel.Synopsis,
                    authorName = authorName,
                    averageRating = novel.AverageRating,
                    totalChapters = novel.TotalChapters,
                    totalRatings = novel.TotalRatings,
                    viewCount = novel.ViewCount,
                    bookmarkCount = novel.BookmarkCount,
                    status = novel.Status,
                    isPremium = novel.IsPremium,
                    publishDate = novel.PublishDate.ToString("MMM dd, yyyy"),
                    lastUpdated = novel.LastUpdated.ToString("MMM dd, yyyy"),
                    language = novel.Language,
                    genres = genres,
                    latestChapters = latestChapters,
                    hasImage = novel.HasCoverImage,
                    coverImageUrl = novel.HasCoverImage ? Url.Action("GetCoverImage", "Home", new { id = novel.Id }) : null
                };

                return Json(new { success = true, novel = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Serves cover images for novels
        /// </summary>
        [HttpGet]
        public ActionResult GetCoverImage(int id)
        {
            try
            {
                var novel = db.Novels.Find(id);
                if (novel?.CoverImage != null && novel.CoverImage.Length > 0)
                {
                    return File(novel.CoverImage, novel.CoverImageContentType ?? "image/jpeg");
                }

                // Return default placeholder
                string defaultImagePath = Server.MapPath("~/Content/images/no-cover-placeholder.jpg");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    return File(defaultImagePath, "image/jpeg");
                }

                return new HttpStatusCodeResult(404);
            }
            catch
            {
                return new HttpStatusCodeResult(500);
            }
        }

        #endregion

        #region Debug/Development Methods (Remove in Production)

        /// <summary>
        /// Basic novel statistics for debugging
        /// </summary>
        [HttpGet]
        public JsonResult DebugNovels()
        {
            try
            {
                var totalNovels = db.Novels.Count();
                var activeNovels = db.Novels.Count(n => n.IsActive);
                var approvedNovels = db.Novels.Count(n => n.ModerationStatus == "Approved");
                var sliderFeatured = db.Novels.Count(n => n.IsSliderFeatured);

                return Json(new
                {
                    totalNovels = totalNovels,
                    activeNovels = activeNovels,
                    approvedNovels = approvedNovels,
                    sliderFeatured = sliderFeatured
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Check specific novel details for debugging
        /// </summary>
        [HttpGet]
        public JsonResult CheckNovelDetails(int id)
        {
            try
            {
                var novel = db.Novels.Find(id);
                if (novel == null)
                {
                    return Json(new { success = false, message = "Novel not found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    novel = new
                    {
                        id = novel.Id,
                        title = novel.Title,
                        isActive = novel.IsActive,
                        moderationStatus = novel.ModerationStatus,
                        isSliderFeatured = novel.IsSliderFeatured,
                        averageRating = novel.AverageRating,
                        viewCount = novel.ViewCount,
                        lastUpdated = novel.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}