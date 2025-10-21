using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Controllers
{
    public class ChapterReaderController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: ChapterReader/Read/{id}
        [HttpGet]
        public ActionResult Read(int id)
        {
            Debug.WriteLine($"[DEBUG] ChapterReader Read action called with chapter ID: {id}");
            return ProcessChapterReading(id);
        }

        [HttpGet]
        public ActionResult ReadBySlug(string novelSlug, int chapterNumber)
        {
            Debug.WriteLine($"[DEBUG] ReadBySlug called with novelSlug: {novelSlug}, chapterNumber: {chapterNumber}");

            try
            {
                // First try to find by existing slug
                var novel = db.Novels.FirstOrDefault(n => n.Slug == novelSlug && n.IsActive);

                // If not found by slug, try to find by generated slug from title
                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] Novel not found by slug '{novelSlug}', trying by title match");
                    var allActiveNovels = db.Novels.Where(n => n.IsActive).ToList();
                    novel = allActiveNovels.FirstOrDefault(n => SlugHelper.GenerateSlug(n.Title) == novelSlug);
                }

                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] Novel with slug '{novelSlug}' not found");
                    return HttpNotFound();
                }

                Debug.WriteLine($"[DEBUG] Found novel: {novel.Title} (ID: {novel.Id})");

                // Then find the chapter
                var chapter = db.Chapters.FirstOrDefault(c => c.NovelId == novel.Id && c.ChapterNumber == chapterNumber);
                if (chapter == null)
                {
                    Debug.WriteLine($"[DEBUG] Chapter {chapterNumber} not found for novel {novelSlug}");
                    return HttpNotFound();
                }

                Debug.WriteLine($"[DEBUG] Found chapter: {chapter.Title} (ID: {chapter.Id})");

                // Process the chapter reading directly (same logic as Read action)
                return ProcessChapterReading(chapter.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in ReadBySlug: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Extract the main logic from Read action into a separate method
        private ActionResult ProcessChapterReading(int chapterId)
        {
            Debug.WriteLine($"[DEBUG] ProcessChapterReading called with chapter ID: {chapterId}");

            try
            {
                var chapter = db.Chapters
                    .Where(c => c.Id == chapterId)
                    .Select(c => new
                    {
                        Id = c.Id,
                        ChapterNumber = c.ChapterNumber,
                        Title = c.Title,
                        Content = c.Content,
                        PublishDate = c.PublishDate,
                        ViewCount = c.ViewCount,
                        IsPremium = c.IsPremium,
                        UnlockPrice = c.UnlockPrice,
                        NovelId = c.NovelId,
                        Novel = new
                        {
                            Id = c.Novel.Id,
                            Title = c.Novel.Title,
                            AuthorId = c.Novel.AuthorId
                        }
                    })
                    .FirstOrDefault();

                if (chapter == null)
                {
                    Debug.WriteLine($"[DEBUG] Chapter with ID {chapterId} not found");
                    return HttpNotFound();
                }

                // Get novel details
                var novel = db.Novels.Find(chapter.NovelId);
                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] Novel not found for chapter {chapterId}");
                    return HttpNotFound();
                }

                // Load author
                var author = db.Authors.Find(novel.AuthorId);
                if (author != null && author.UserId != 0)
                {
                    author.User = db.Users.Find(author.UserId);
                }

                // Check if user can access this chapter
                int? currentUserId = GetCurrentUserId();
                bool canAccess = !chapter.IsPremium || IsChapterUnlocked(chapterId, currentUserId);

                if (!canAccess)
                {
                    TempData["ErrorMessage"] = $"This chapter requires {chapter.UnlockPrice} coins to unlock.";
                    return RedirectToAction("BookDetail", "Book", new { id = chapter.NovelId });
                }

                // Get navigation chapters
                var navigationInfo = GetNavigationChapters(chapter.NovelId, chapter.ChapterNumber);

                var viewModel = new ChapterReadViewModel
                {
                    ChapterId = chapter.Id,
                    ChapterNumber = chapter.ChapterNumber,
                    ChapterTitle = chapter.Title,
                    Content = chapter.Content,
                    PublishDate = chapter.PublishDate,
                    ViewCount = chapter.ViewCount,

                    NovelId = chapter.NovelId,
                    NovelTitle = novel.Title,

                    AuthorName = author?.PenName ?? "Unknown Author",
                    AuthorId = novel.AuthorId,

                    PreviousChapter = navigationInfo.PreviousChapter,
                    NextChapter = navigationInfo.NextChapter,
                    TotalChapters = navigationInfo.TotalChapters
                };

                // Update view count and reading progress
                UpdateChapterStats(chapterId, currentUserId, chapter.NovelId, chapter.ChapterNumber);

                Debug.WriteLine($"[DEBUG] Chapter reader view model created for: {chapter.Title}");
                return View("Read", viewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in ProcessChapterReading: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpGet]
        public ActionResult PopulateSlugs()
        {
            try
            {
                var novelsWithoutSlugs = db.Novels.Where(n => n.Slug == null || n.Slug == "").ToList();

                foreach (var novel in novelsWithoutSlugs)
                {
                    novel.Slug = SlugHelper.GenerateSlug(novel.Title);
                    Debug.WriteLine($"Generated slug '{novel.Slug}' for novel: {novel.Title}");
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Updated {novelsWithoutSlugs.Count} novels with slugs"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Update the navigation method to return slug-based URLs
        private (string PreviousChapterUrl, string NextChapterUrl, int TotalChapters) GetNavigationUrls(int novelId, int currentChapterNumber)
        {
            var novel = db.Novels.Find(novelId);
            if (novel == null) return (null, null, 0);

            var chapters = db.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderBy(c => c.ChapterNumber)
                .Select(c => c.ChapterNumber)
                .ToList();

            int currentIndex = chapters.IndexOf(currentChapterNumber);

            string previousUrl = null;
            string nextUrl = null;

            if (currentIndex > 0)
            {
                int prevChapterNumber = chapters[currentIndex - 1];
                previousUrl = $"/read/{novel.Slug}/chapter-{prevChapterNumber}";
            }

            if (currentIndex < chapters.Count - 1)
            {
                int nextChapterNumber = chapters[currentIndex + 1];
                nextUrl = $"/read/{novel.Slug}/chapter-{nextChapterNumber}";
            }

            return (previousUrl, nextUrl, chapters.Count);
        }

        // GET: ChapterReader/Read/{novelId}/{chapterNumber}
        [HttpGet]
        public ActionResult ReadByNovelAndChapter(int novelId, int chapterNumber)
        {
            Debug.WriteLine($"[DEBUG] ChapterReader ReadByNovelAndChapter action called with novelId: {novelId}, chapterNumber: {chapterNumber}");

            try
            {
                var chapter = db.Chapters
                    .Where(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber)
                    .FirstOrDefault();

                if (chapter == null)
                {
                    Debug.WriteLine($"[DEBUG] Chapter {chapterNumber} not found for novel {novelId}");
                    return HttpNotFound();
                }

                // Redirect to the main Read action with chapter ID
                return RedirectToAction("Read", new { id = chapter.Id });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in ChapterReader ReadByNovelAndChapter: {ex.Message}");
                throw;
            }
        }

        private (int? PreviousChapter, int? NextChapter, int TotalChapters) GetNavigationChapters(int novelId, int currentChapterNumber)
        {
            var chapters = db.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderBy(c => c.ChapterNumber)
                .Select(c => new { Id = c.Id, ChapterNumber = c.ChapterNumber })
                .ToList();

            int currentIndex = chapters.FindIndex(c => c.ChapterNumber == currentChapterNumber);

            return (
                PreviousChapter: currentIndex > 0 ? chapters[currentIndex - 1].Id : (int?)null,
                NextChapter: currentIndex < chapters.Count - 1 ? chapters[currentIndex + 1].Id : (int?)null,
                TotalChapters: chapters.Count
            );
        }


        private void UpdateChapterStats(int chapterId, int? userId, int novelId, int chapterNumber)
        {
            try
            {
                // Update chapter view count
                var sql = "UPDATE Chapters SET ViewCount = ViewCount + 1 WHERE Id = @chapterId";
                db.Database.ExecuteSqlCommand(sql,
                    new System.Data.SqlClient.SqlParameter("@chapterId", chapterId));

                // Update reading progress if user is logged in
                if (userId.HasValue)
                {
                    var progress = db.ReadingProgress.FirstOrDefault(rp =>
                        rp.NovelId == novelId && rp.ReaderId == userId.Value);

                    if (progress == null)
                    {
                        db.ReadingProgress.Add(new ReadingProgress
                        {
                            NovelId = novelId,
                            ReaderId = userId.Value,
                            LastReadChapterId = chapterId,
                            LastReadChapterNumber = chapterNumber,
                            LastReadDate = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    else
                    {
                        if (chapterNumber > progress.LastReadChapterNumber)
                        {
                            progress.LastReadChapterId = chapterId;
                            progress.LastReadChapterNumber = chapterNumber;
                            progress.LastReadDate = DateTime.Now;
                            progress.UpdatedAt = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                }

                Debug.WriteLine($"[DEBUG] Chapter stats updated for chapter ID: {chapterId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error updating chapter stats: {ex.Message}");
            }
        }

        private int? GetCurrentUserId()
        {
            // Implement based on your authentication system
            Debug.WriteLine("[DEBUG] GetCurrentUserId called - returning null (not implemented)");
            return null; // Placeholder
        }

        private bool IsChapterUnlocked(int chapterId, int? userId)
        {
            // Implement your chapter unlock logic here
            return true; // Placeholder
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