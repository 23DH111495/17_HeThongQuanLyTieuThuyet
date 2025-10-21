using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Areas.Admin.Controllers
{
    public class Chapter_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public ActionResult Chapter_Manager(string search = "", string sortBy = "created", string sortDirection = "desc",
                                  string statusFilter = "all", string activeFilter = "all", int page = 1, int pageSize = 10)
        {
            try
            {
                // Use LEFT JOIN to handle orphaned AuthorId references
                var query = from n in db.Novels
                            join a in db.Authors on n.AuthorId equals a.Id into authorJoin
                            from author in authorJoin.DefaultIfEmpty()
                            select new
                            {
                                Novel = n,
                                Author = author
                            };

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x => x.Novel.Title.Contains(search) ||
                                           (x.Author != null && x.Author.PenName.Contains(search)) ||
                                           (x.Novel.AlternativeTitle != null && x.Novel.AlternativeTitle.Contains(search)));
                }

                // Apply status filter
                if (statusFilter != "all")
                {
                    query = query.Where(x => x.Novel.Status.ToLower() == statusFilter.ToLower());
                }

                // Apply active filter
                if (activeFilter == "active")
                {
                    query = query.Where(x => x.Novel.IsActive == true);
                }
                else if (activeFilter == "inactive")
                {
                    query = query.Where(x => x.Novel.IsActive == false);
                }

                // Apply sorting
                switch (sortBy.ToLower())
                {
                    case "id":
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Novel.Id)
                            : query.OrderByDescending(x => x.Novel.Id);
                        break;
                    case "title":
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Novel.Title)
                            : query.OrderByDescending(x => x.Novel.Title);
                        break;
                    case "author":
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Author != null ? x.Author.PenName : "")
                            : query.OrderByDescending(x => x.Author != null ? x.Author.PenName : "");
                        break;
                    case "status":
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Novel.Status)
                            : query.OrderByDescending(x => x.Novel.Status);
                        break;
                    case "chapters":
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Novel.TotalChapters)
                            : query.OrderByDescending(x => x.Novel.TotalChapters);
                        break;
                    default:
                        query = sortDirection == "asc"
                            ? query.OrderBy(x => x.Novel.CreatedAt)
                            : query.OrderByDescending(x => x.Novel.CreatedAt);
                        break;
                }

                var totalNovels = query.Count();

                // Get the results and map them back to Novel objects
                var results = query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

                var novels = new List<Novel>();
                foreach (var result in results)
                {
                    var novel = result.Novel;
                    novel.Author = result.Author; // Manually assign the author (can be null)
                    novels.Add(novel);
                }

                // Set ViewBag properties
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalNovels / pageSize);
                ViewBag.TotalCount = totalNovels;  // Use the filtered count
                ViewBag.PageSize = pageSize;  // Add this line
                ViewBag.FilteredCount = totalNovels;
                ViewBag.Search = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDirection = sortDirection;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.ActiveFilter = activeFilter;
                ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) ||
                                          statusFilter != "all" ||
                                          activeFilter != "all";

                return View(novels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Chapter_Manager: {ex.Message}");
                ViewBag.ErrorMessage = $"An error occurred while loading novels: {ex.Message}";
                return View(new List<Novel>());
            }
        }

        #region Create Chapter Code Controller
        public ActionResult CreateChapter(int? novelId)
        {
            ViewBag.Novels = new SelectList(db.Novels.OrderBy(n => n.Title), "Id", "Title", novelId);

            var chapter = new Chapter();
            if (novelId.HasValue)
            {
                chapter.NovelId = novelId.Value;
                var lastChapter = db.Chapters.Where(c => c.NovelId == novelId.Value)
                                            .OrderByDescending(c => c.ChapterNumber)
                                            .FirstOrDefault();
                chapter.ChapterNumber = lastChapter == null ? 1 : (lastChapter.ChapterNumber + 1);
            }

            return View(chapter);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateChapter(Chapter chapter)
        {
            if (ModelState.IsValid)
            {
                var existingChapter = db.Chapters.FirstOrDefault(c => c.NovelId == chapter.NovelId
                                                                   && c.ChapterNumber == chapter.ChapterNumber);
                if (existingChapter != null)
                {
                    ModelState.AddModelError("ChapterNumber", "Chapter number already exists for this novel.");
                }
                else
                {
                    chapter.WordCount = CountWords(chapter.Content);
                    chapter.CreatedAt = DateTime.Now;
                    chapter.UpdatedAt = DateTime.Now;
                    chapter.ModerationStatus = "Approved";

                    db.Chapters.Add(chapter);
                    db.SaveChanges();

                    UpdateNovelStats(chapter.NovelId);

                    TempData["SuccessMessage"] = "Chapter created successfully.";
                    return RedirectToAction("DetailChapter", new { novelId = chapter.NovelId });
                }
            }

            ViewBag.Novels = new SelectList(db.Novels.OrderBy(n => n.Title), "Id", "Title", chapter.NovelId);
            return View(chapter);
        }
        #endregion

        public ActionResult DetailChapter(int? novelId = null, string search = "", string sortBy = "ChapterNumber",
                                        string sortDirection = "desc", string statusFilter = "", string typeFilter = "",
                                        int page = 1, int pageSize = 20)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DetailChapter Enhanced Debug ===");
                System.Diagnostics.Debug.WriteLine($"novelId parameter: {novelId}");

                var totalNovelsInDb = db.Novels.Count();
                var totalChaptersInDb = db.Chapters.Count();
                System.Diagnostics.Debug.WriteLine($"Total novels in DB: {totalNovelsInDb}");
                System.Diagnostics.Debug.WriteLine($"Total chapters in DB: {totalChaptersInDb}");

                // Get the current novel first (using your original LEFT JOIN approach)
                Novel currentNovel = null;

                if (novelId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Filtering by novelId: {novelId}");

                    // Get the novel using the same LEFT JOIN approach as your original code
                    var basicNovel = db.Novels.FirstOrDefault(n => n.Id == novelId.Value);
                    System.Diagnostics.Debug.WriteLine($"Basic novel query result: {basicNovel?.Title ?? "NULL"}");

                    if (basicNovel != null)
                    {
                        var novelWithAuthor = (from n in db.Novels
                                               join a in db.Authors on n.AuthorId equals a.Id into authorJoin
                                               from author in authorJoin.DefaultIfEmpty()
                                               where n.Id == novelId.Value
                                               select new { Novel = n, Author = author }).FirstOrDefault();

                        if (novelWithAuthor != null)
                        {
                            currentNovel = novelWithAuthor.Novel;
                            currentNovel.Author = novelWithAuthor.Author;
                            System.Diagnostics.Debug.WriteLine($"Novel with LEFT JOIN found: {currentNovel.Title}");
                            System.Diagnostics.Debug.WriteLine($"Author: {currentNovel.Author?.PenName ?? "No Author"}");
                        }
                        else
                        {
                            currentNovel = basicNovel;
                            System.Diagnostics.Debug.WriteLine($"Using basic novel without author: {currentNovel.Title}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Novel with ID {novelId} not found");
                        TempData["ErrorMessage"] = $"Novel with ID {novelId} not found.";
                        return RedirectToAction("Chapter_Manager");
                    }
                }

                // FIXED: Build a proper query for chapters that preserves EF tracking
                // Start with a base query that will maintain proper tracking
                var chaptersQuery = from c in db.Chapters
                                    select c;

                if (novelId.HasValue)
                {
                    chaptersQuery = chaptersQuery.Where(c => c.NovelId == novelId.Value);
                    System.Diagnostics.Debug.WriteLine($"Applied novelId filter to chapters query");
                }

                // DEBUG: Check chapters count after novel filter
                var chaptersAfterNovelFilter = chaptersQuery.Count();
                System.Diagnostics.Debug.WriteLine($"Chapters after novel filter: {chaptersAfterNovelFilter}");

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    System.Diagnostics.Debug.WriteLine($"Applying search filter: '{search}'");
                    chaptersQuery = chaptersQuery.Where(c => c.Title.Contains(search) ||
                                                            c.ChapterNumber.ToString().Contains(search));
                    var chaptersAfterSearch = chaptersQuery.Count();
                    System.Diagnostics.Debug.WriteLine($"Chapters after search filter: {chaptersAfterSearch}");
                }

                // Apply status filter
                if (statusFilter == "published")
                {
                    System.Diagnostics.Debug.WriteLine($"Applying status filter: published");
                    chaptersQuery = chaptersQuery.Where(c => c.IsPublished == true);
                }
                else if (statusFilter == "draft")
                {
                    System.Diagnostics.Debug.WriteLine($"Applying status filter: draft");
                    chaptersQuery = chaptersQuery.Where(c => c.IsPublished == false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No status filter applied (statusFilter='{statusFilter}')");
                }

                var chaptersAfterStatusFilter = chaptersQuery.Count();
                System.Diagnostics.Debug.WriteLine($"Chapters after status filter: {chaptersAfterStatusFilter}");

                // Apply type filter
                if (typeFilter == "premium")
                {
                    System.Diagnostics.Debug.WriteLine($"Applying type filter: premium");
                    chaptersQuery = chaptersQuery.Where(c => c.IsPremium == true);
                }
                else if (typeFilter == "early")
                {
                    System.Diagnostics.Debug.WriteLine($"Applying type filter: early access");
                    chaptersQuery = chaptersQuery.Where(c => c.IsEarlyAccess == true);
                }
                else if (typeFilter == "regular")
                {
                    System.Diagnostics.Debug.WriteLine($"Applying type filter: regular");
                    chaptersQuery = chaptersQuery.Where(c => c.IsPremium == false && c.IsEarlyAccess == false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No type filter applied (typeFilter='{typeFilter}')");
                }

                var chaptersAfterTypeFilter = chaptersQuery.Count();
                System.Diagnostics.Debug.WriteLine($"Chapters after type filter: {chaptersAfterTypeFilter}");

                // Apply sorting
                System.Diagnostics.Debug.WriteLine($"Applying sort: {sortBy} {sortDirection}");
                switch (sortBy.ToLower())
                {
                    case "chapternumber":
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.ChapterNumber)
                            : chaptersQuery.OrderByDescending(c => c.ChapterNumber);
                        break;
                    case "title":
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.Title)
                            : chaptersQuery.OrderByDescending(c => c.Title);
                        break;
                    case "novel":
                        // Can't sort by novel title here since we're not including Novel in the query
                        // Fall back to default sorting
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.CreatedAt)
                            : chaptersQuery.OrderByDescending(c => c.CreatedAt);
                        break;
                    case "wordcount":
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.WordCount)
                            : chaptersQuery.OrderByDescending(c => c.WordCount);
                        break;
                    case "viewcount":
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.ViewCount)
                            : chaptersQuery.OrderByDescending(c => c.ViewCount);
                        break;
                    case "updatedat":
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.UpdatedAt)
                            : chaptersQuery.OrderByDescending(c => c.UpdatedAt);
                        break;
                    default:
                        chaptersQuery = sortDirection == "asc"
                            ? chaptersQuery.OrderBy(c => c.CreatedAt)
                            : chaptersQuery.OrderByDescending(c => c.CreatedAt);
                        break;
                }

                var totalChapters = chaptersQuery.Count();
                System.Diagnostics.Debug.WriteLine($"Final chapter count before pagination: {totalChapters}");

                // Debug pagination
                System.Diagnostics.Debug.WriteLine($"Pagination: page={page}, pageSize={pageSize}");
                System.Diagnostics.Debug.WriteLine($"Skip: {(page - 1) * pageSize}, Take: {pageSize}");

                // FIXED: Get the basic chapters first, then load related data separately
                var chapters = chaptersQuery.Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();

                // Now load the Novel and Author data for each chapter
                foreach (var chapter in chapters)
                {
                    // Load novel data using the same LEFT JOIN approach
                    var novelWithAuthor = (from n in db.Novels
                                           join a in db.Authors on n.AuthorId equals a.Id into authorJoin
                                           from author in authorJoin.DefaultIfEmpty()
                                           where n.Id == chapter.NovelId
                                           select new { Novel = n, Author = author }).FirstOrDefault();

                    if (novelWithAuthor != null)
                    {
                        chapter.Novel = novelWithAuthor.Novel;
                        chapter.Novel.Author = novelWithAuthor.Author;
                    }
                    else
                    {
                        // Fallback: load just the novel without author
                        chapter.Novel = db.Novels.FirstOrDefault(n => n.Id == chapter.NovelId);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Final results:");
                System.Diagnostics.Debug.WriteLine($"- Chapters returned after pagination: {chapters.Count}");
                System.Diagnostics.Debug.WriteLine($"- Current novel: {currentNovel?.Title ?? "NULL"}");

                // Debug: Show the actual chapters returned WITH THEIR IDs
                System.Diagnostics.Debug.WriteLine($"Chapters returned with IDs:");
                foreach (var chapter in chapters)
                {
                    System.Diagnostics.Debug.WriteLine($"  ID: {chapter.Id} | Chapter {chapter.ChapterNumber}: {chapter.Title ?? "No Title"} - Novel: {chapter.Novel?.Title}");
                }

                System.Diagnostics.Debug.WriteLine($"=== End Enhanced Debug ===");

                // Calculate time since last update
                string timeSinceUpdate = "Never";
                if (currentNovel?.LastUpdated != null)
                {
                    var timeDiff = DateTime.Now - currentNovel.LastUpdated;
                    if (timeDiff.TotalMinutes < 60)
                    {
                        timeSinceUpdate = $"{(int)timeDiff.TotalMinutes} minutes ago";
                    }
                    else if (timeDiff.TotalHours < 24)
                    {
                        timeSinceUpdate = $"{(int)timeDiff.TotalHours} hours ago";
                    }
                    else
                    {
                        timeSinceUpdate = $"{(int)timeDiff.TotalDays} days ago";
                    }
                }

                // Set ViewBag properties
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalChapters / pageSize);
                ViewBag.TotalCount = db.Chapters.Count();
                ViewBag.FilteredCount = totalChapters;
                ViewBag.PageSize = pageSize;

                ViewBag.Search = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDirection = sortDirection;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TypeFilter = typeFilter;
                ViewBag.NovelId = novelId;

                ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) ||
                                          statusFilter != "" ||
                                          typeFilter != "" ||
                                          novelId.HasValue;

                // Pass novel info to view
                ViewBag.CurrentNovel = currentNovel;
                ViewBag.TimeSinceUpdate = timeSinceUpdate;

                return View(chapters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DetailChapter: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Chapter_Manager");
            }
        }

        public ActionResult PublishChapter(int id)
        {
            var chapter = db.Chapters.Find(id);
            if (chapter != null)
            {
                chapter.IsPublished = true;
                chapter.PublishDate = DateTime.Now;
                chapter.UpdatedAt = DateTime.Now;

                // Update the novel's last updated time
                var novel = db.Novels.Find(chapter.NovelId);
                if (novel != null)
                {
                    novel.LastUpdated = DateTime.Now;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Chapter published successfully.";

                // Fixed: Redirect back to the same novel's chapter list
                return RedirectToAction("DetailChapter", new { novelId = chapter.NovelId });
            }

            // Fallback: redirect to chapter manager if chapter not found
            return RedirectToAction("Chapter_Manager");
        }

        public ActionResult UnpublishChapter(int id)
        {
            var chapter = db.Chapters.Find(id);
            if (chapter != null)
            {
                chapter.IsPublished = false;
                chapter.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Chapter unpublished successfully.";

                // Fixed: Redirect back to the same novel's chapter list
                return RedirectToAction("DetailChapter", new { novelId = chapter.NovelId });
            }

            // Fallback: redirect to chapter manager if chapter not found
            return RedirectToAction("Chapter_Manager");
        }

        public ActionResult DeleteChapter(int id)
        {
            var chapter = db.Chapters.Find(id);
            if (chapter != null)
            {
                var novelId = chapter.NovelId;
                db.Chapters.Remove(chapter);
                db.SaveChanges();
                UpdateNovelStats(novelId);
                TempData["SuccessMessage"] = "Chapter deleted successfully.";

                // Fixed: Redirect back to the same novel's chapter list
                return RedirectToAction("DetailChapter", new { novelId = novelId });
            }

            return RedirectToAction("Chapter_Manager");
        }

        #region Edit Chapter Code
        public ActionResult EditChapter(int id)
        {
            var chapter = db.Chapters.Include(c => c.Novel).FirstOrDefault(c => c.Id == id);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = "Chapter not found.";
                return RedirectToAction("DetailChapter");
            }
            return View(chapter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditChapter(Chapter chapter)
        {
            if (ModelState.IsValid)
            {
                var existingChapter = db.Chapters.FirstOrDefault(c => c.NovelId == chapter.NovelId
                                                                   && c.ChapterNumber == chapter.ChapterNumber
                                                                   && c.Id != chapter.Id);
                if (existingChapter != null)
                {
                    ModelState.AddModelError("ChapterNumber", "Chapter number already exists for this novel.");
                }
                else
                {
                    var originalChapter = db.Chapters.Find(chapter.Id);
                    if (originalChapter != null)
                    {
                        originalChapter.Title = chapter.Title;
                        originalChapter.Content = chapter.Content;
                        originalChapter.ChapterNumber = chapter.ChapterNumber;
                        originalChapter.IsPublished = chapter.IsPublished;
                        originalChapter.IsPremium = chapter.IsPremium;
                        originalChapter.IsEarlyAccess = chapter.IsEarlyAccess;
                        originalChapter.UnlockPrice = chapter.UnlockPrice;
                        originalChapter.WordCount = CountWords(chapter.Content);
                        originalChapter.UpdatedAt = DateTime.Now;

                        if (chapter.IsPublished && originalChapter.PublishDate == null)
                        {
                            originalChapter.PublishDate = DateTime.Now;
                        }

                        db.SaveChanges();
                        UpdateNovelStats(originalChapter.NovelId);

                        TempData["SuccessMessage"] = "Chapter updated successfully.";
                        return RedirectToAction("DetailChapter", new { novelId = originalChapter.NovelId });
                    }
                }
            }

            chapter = db.Chapters.Include(c => c.Novel).FirstOrDefault(c => c.Id == chapter.Id);
            return View(chapter);
        }
        #endregion
        
        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new char[] { ' ', '\t', '\n', '\r' },
                            StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private void UpdateNovelStats(int novelId)
        {
            var novel = db.Novels.Find(novelId);
            if (novel != null)
            {
                var chapters = db.Chapters.Where(c => c.NovelId == novelId).ToList();

                novel.TotalChapters = chapters.Count;
                novel.WordCount = chapters.Sum(c => c.WordCount);
                novel.LastUpdated = DateTime.Now;

                db.SaveChanges();
            }
        }

        public ActionResult ViewChapter(int id)
        {
            System.Diagnostics.Debug.WriteLine($"=== ViewChapter Debug ===");
            System.Diagnostics.Debug.WriteLine($"Chapter ID parameter: {id}");

            // First, let's see what chapters exist in the database
            var allChapterIds = db.Chapters.Select(c => c.Id).ToList();
            System.Diagnostics.Debug.WriteLine($"All chapter IDs in database: {string.Join(", ", allChapterIds)}");

            // FIXED: Use simple query first, then load related data separately
            var chapter = db.Chapters.FirstOrDefault(c => c.Id == id);

            if (chapter == null)
            {
                System.Diagnostics.Debug.WriteLine($"Chapter with ID {id} not found");
                TempData["ErrorMessage"] = "Chapter not found.";
                return RedirectToAction("Chapter_Manager");
            }

            System.Diagnostics.Debug.WriteLine($"Basic chapter found: {chapter.Title} (ID: {chapter.Id}, Number: {chapter.ChapterNumber})");

            // Load novel and author data using the same LEFT JOIN approach as DetailChapter
            var novelWithAuthor = (from n in db.Novels
                                   join a in db.Authors on n.AuthorId equals a.Id into authorJoin
                                   from author in authorJoin.DefaultIfEmpty()
                                   where n.Id == chapter.NovelId
                                   select new { Novel = n, Author = author }).FirstOrDefault();

            if (novelWithAuthor != null)
            {
                chapter.Novel = novelWithAuthor.Novel;
                chapter.Novel.Author = novelWithAuthor.Author;
                System.Diagnostics.Debug.WriteLine($"Novel loaded: {chapter.Novel.Title}, Author: {chapter.Novel.Author?.PenName ?? "No Author"}");
            }
            else
            {
                // Fallback: load just the novel without author
                chapter.Novel = db.Novels.FirstOrDefault(n => n.Id == chapter.NovelId);
                System.Diagnostics.Debug.WriteLine($"Novel loaded (no author): {chapter.Novel?.Title ?? "No Novel"}");
            }

            System.Diagnostics.Debug.WriteLine($"Final chapter data: {chapter.Title} (ID: {chapter.Id}, Number: {chapter.ChapterNumber}, Novel: {chapter.Novel?.Title})");

            // FIXED: Increment view count using a direct database update to avoid tracking issues
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to increment view count for chapter ID: {chapter.Id}");

                // Use a direct SQL update to avoid EF tracking issues
                var chapterToUpdate = db.Chapters.Find(chapter.Id);
                if (chapterToUpdate != null)
                {
                    chapterToUpdate.ViewCount++;
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"View count incremented successfully to: {chapterToUpdate.ViewCount}");

                    // Update our display object with the new view count
                    chapter.ViewCount = chapterToUpdate.ViewCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error incrementing view count: {ex.Message}");
                // Don't let view count update failure break the page - just log it
            }

            // Get navigation info
            var navigationInfo = GetChapterNavigation(chapter.NovelId, chapter.ChapterNumber);
            ViewBag.NavigationInfo = navigationInfo;

            System.Diagnostics.Debug.WriteLine($"=== End ViewChapter Debug ===");

            return View(chapter);
        }

        public ActionResult ViewChapterByNumber(int novelId, int chapterNumber)
        {
            System.Diagnostics.Debug.WriteLine($"=== ViewChapterByNumber Debug ===");
            System.Diagnostics.Debug.WriteLine($"NovelId: {novelId}, ChapterNumber: {chapterNumber}");

            // Use simple query first
            var chapter = db.Chapters.FirstOrDefault(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);

            if (chapter == null)
            {
                System.Diagnostics.Debug.WriteLine($"Chapter not found for NovelId: {novelId}, ChapterNumber: {chapterNumber}");
                TempData["ErrorMessage"] = "Chapter not found.";
                return RedirectToAction("DetailChapter", new { novelId = novelId });
            }

            System.Diagnostics.Debug.WriteLine($"Chapter found: {chapter.Title} (ID: {chapter.Id})");

            // Load novel and author data using LEFT JOIN approach
            var novelWithAuthor = (from n in db.Novels
                                   join a in db.Authors on n.AuthorId equals a.Id into authorJoin
                                   from author in authorJoin.DefaultIfEmpty()
                                   where n.Id == chapter.NovelId
                                   select new { Novel = n, Author = author }).FirstOrDefault();

            if (novelWithAuthor != null)
            {
                chapter.Novel = novelWithAuthor.Novel;
                chapter.Novel.Author = novelWithAuthor.Author;
            }
            else
            {
                chapter.Novel = db.Novels.FirstOrDefault(n => n.Id == chapter.NovelId);
            }

            // FIXED: Increment view count using proper EF tracking
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to increment view count for chapter ID: {chapter.Id}");

                var chapterToUpdate = db.Chapters.Find(chapter.Id);
                if (chapterToUpdate != null)
                {
                    chapterToUpdate.ViewCount++;
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"View count incremented successfully");

                    // Update our display object
                    chapter.ViewCount = chapterToUpdate.ViewCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error incrementing view count: {ex.Message}");
                // Continue without failing
            }

            // Get navigation info
            var navigationInfo = GetChapterNavigation(novelId, chapterNumber);
            ViewBag.NavigationInfo = navigationInfo;

            System.Diagnostics.Debug.WriteLine($"=== End ViewChapterByNumber Debug ===");

            return View("ViewChapter", chapter);
        }

        private ChapterNavigationInfo GetChapterNavigation(int novelId, int currentChapterNumber)
        {
            var navigationInfo = new ChapterNavigationInfo();

            var chapterNumbers = db.Chapters
                                  .Where(c => c.NovelId == novelId)
                                  .OrderBy(c => c.ChapterNumber)
                                  .Select(c => c.ChapterNumber)
                                  .ToList();

            if (chapterNumbers.Any())
            {
                var currentIndex = chapterNumbers.IndexOf(currentChapterNumber);

                if (currentIndex > 0)
                {
                    navigationInfo.PreviousChapterNumber = chapterNumbers[currentIndex - 1];
                    navigationInfo.HasPrevious = true;
                }

                if (currentIndex < chapterNumbers.Count - 1)
                {
                    navigationInfo.NextChapterNumber = chapterNumbers[currentIndex + 1];
                    navigationInfo.HasNext = true;
                }

                navigationInfo.CurrentPosition = currentIndex + 1;
                navigationInfo.TotalChapters = chapterNumbers.Count;
            }

            navigationInfo.NovelId = novelId;
            navigationInfo.CurrentChapterNumber = currentChapterNumber;

            return navigationInfo;
        }

        public ActionResult DeleteConfirmation(int id)
        {
            var chapter = db.Chapters.Include(c => c.Novel).FirstOrDefault(c => c.Id == id);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = "Chapter not found.";
                return RedirectToAction("DetailChapter");
            }
            return View(chapter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(int id)
        {
            var chapter = db.Chapters.Find(id);
            if (chapter != null)
            {
                var novelId = chapter.NovelId;
                db.Chapters.Remove(chapter);
                db.SaveChanges();
                UpdateNovelStats(novelId);
                TempData["SuccessMessage"] = "Chapter deleted successfully.";
            }
            return RedirectToAction("DetailChapter");
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