using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Areas.Admin.Controllers.Novel_ManagerController
{
    public class Novel_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public ActionResult Novel_Manager(string search = "", string statusFilter = "all",
            string moderationFilter = "all", string activeFilter = "all",
            string sortBy = "created", string sortDirection = "desc", int page = 1)
        {
            try
            {
                int pageSize = 10;

                // Use LEFT JOIN instead of INNER JOIN for Author relationship
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
                    var searchTerm = search.ToLower().Trim();
                    query = query.Where(x =>
                        x.Novel.Title.ToLower().Contains(searchTerm) ||
                        (x.Author != null && x.Author.PenName.ToLower().Contains(searchTerm)) ||
                        (x.Novel.AlternativeTitle != null && x.Novel.AlternativeTitle.ToLower().Contains(searchTerm)) ||
                        (x.Novel.Synopsis != null && x.Novel.Synopsis.ToLower().Contains(searchTerm))
                    );
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                {
                    query = query.Where(x => x.Novel.Status.ToLower() == statusFilter.ToLower());
                }

                // Apply moderation filter
                if (!string.IsNullOrEmpty(moderationFilter) && moderationFilter != "all")
                {
                    query = query.Where(x => x.Novel.ModerationStatus.ToLower() == moderationFilter.ToLower());
                }

                // Apply active filter
                if (!string.IsNullOrEmpty(activeFilter) && activeFilter != "all")
                {
                    bool isActive = activeFilter.ToLower() == "active";
                    query = query.Where(x => x.Novel.IsActive == isActive);
                }

                // Apply sorting
                switch (sortBy?.ToLower())
                {
                    case "id":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.Novel.Id)
                            : query.OrderBy(x => x.Novel.Id);
                        break;
                    case "title":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.Novel.Title)
                            : query.OrderBy(x => x.Novel.Title);
                        break;
                    case "author":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.Author != null ? x.Author.PenName : "")
                            : query.OrderBy(x => x.Author != null ? x.Author.PenName : "");
                        break;
                    case "status":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.Novel.Status)
                            : query.OrderBy(x => x.Novel.Status);
                        break;
                    case "moderation":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.Novel.ModerationStatus)
                            : query.OrderBy(x => x.Novel.ModerationStatus);
                        break;
                    case "created":
                    default:
                        query = sortDirection?.ToLower() == "asc"
                            ? query.OrderBy(x => x.Novel.CreatedAt)
                            : query.OrderByDescending(x => x.Novel.CreatedAt);
                        break;
                }

                int totalCount = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Get the results and map them back to Novel objects
                var results = query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

                var novels = new List<Novel>();
                foreach (var result in results)
                {
                    var novel = result.Novel;
                    novel.Author = result.Author; // Manually assign the author
                    novels.Add(novel);
                }

                // Process additional data for each novel
                foreach (var novel in novels)
                {
                    // Load chapters if needed
                    if (novel.TotalChapters == 0)
                    {
                        novel.TotalChapters = db.Chapters.Where(c => c.NovelId == novel.Id).Count();
                    }

                    // Load genres manually to avoid include issues
                    var novelGenres = db.NovelGenres
                        .Where(ng => ng.NovelId == novel.Id)
                        .Include(ng => ng.Genre)
                        .ToList();

                    novel.Genres = novelGenres.Select(ng => ng.Genre).ToList();

                    // Set default values
                    if (novel.ViewCount == null) novel.ViewCount = 0;
                    if (novel.AverageRating == null) novel.AverageRating = 0;
                    if (novel.TotalRatings == null) novel.TotalRatings = 0;
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.ModerationFilter = moderationFilter;
                ViewBag.ActiveFilter = activeFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDirection = sortDirection;

                ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) ||
                                          statusFilter != "all" ||
                                          moderationFilter != "all" ||
                                          activeFilter != "all";

                ViewBag.FilteredCount = totalCount;

                return View(novels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Novel_Manager: {ex.Message}");
                ViewBag.ErrorMessage = $"An error occurred while loading novels: {ex.Message}";
                return View(new List<Novel>());
            }
        }

        #region Delete Controller 
        [HttpGet]
        public ActionResult DeleteNovel(int id, string search = "", string statusFilter = "all",
            string moderationFilter = "all", string activeFilter = "all", int page = 1)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DeleteNovel GET called with id: {id}");

                // Validate id parameter
                if (id <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid ID: {id}");
                    TempData["ErrorMessage"] = "Invalid novel ID.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Load novel - simplified approach without Include
                var novel = db.Novels.FirstOrDefault(n => n.Id == id);

                if (novel == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Novel with ID {id} not found in database");
                    TempData["ErrorMessage"] = "Novel not found.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Load author separately
                if (novel.AuthorId > 0)
                {
                    novel.Author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);
                }

                // Load chapters count
                var chaptersCount = db.Chapters.Where(c => c.NovelId == id).Count();
                ViewBag.ChaptersCount = chaptersCount;

                // Store return parameters
                ViewBag.Search = search;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.ModerationFilter = moderationFilter;
                ViewBag.ActiveFilter = activeFilter;
                ViewBag.CurrentPage = page;
                ViewBag.ReturnUrl = Url.Action("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });

                System.Diagnostics.Debug.WriteLine($"Successfully loaded novel for deletion: {novel.Title} (ID: {novel.Id})");
                return View(novel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteNovel GET: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading delete confirmation: " + ex.Message;
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNovelConfirmed(int id, string search = "", string statusFilter = "all",
        string moderationFilter = "all", string activeFilter = "all", int page = 1)
        {
            try
            {
                var novel = db.Novels
                    .Include(n => n.Chapters)
                    .Include(n => n.NovelGenres)
                    .Include(n => n.NovelTags)
                    .FirstOrDefault(n => n.Id == id);
                if (novel == null)
                {
                    TempData["ErrorMessage"] = "Novel not found.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }
                string novelTitle = novel.Title;
                if (novel.NovelGenres != null && novel.NovelGenres.Any())
                {
                    db.NovelGenres.RemoveRange(novel.NovelGenres);
                }
                if (novel.NovelTags != null && novel.NovelTags.Any())
                {
                    db.NovelTags.RemoveRange(novel.NovelTags);
                }
                if (novel.Chapters != null && novel.Chapters.Any())
                {
                    var chapterIds = novel.Chapters.Select(c => c.Id).ToList();
                    if (novel.Chapters != null && novel.Chapters.Any())
                    {
                        db.Chapters.RemoveRange(novel.Chapters);
                    }
                }
                if (!string.IsNullOrEmpty(novel.CoverImageUrl) && novel.CoverImageUrl.StartsWith("/Content/uploads/"))
                {
                    try
                    {
                        string filePath = Server.MapPath("~" + novel.CoverImageUrl);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                db.Novels.Remove(novel);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Novel '{novelTitle}' has been permanently deleted along with all associated data.";
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting novel: " + ex.Message;
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatusForm(int id, string search = "", string statusFilter = "all",
            string moderationFilter = "all", string activeFilter = "all", int page = 1)
        {
            try
            {
                var novel = db.Novels.Find(id);
                if (novel == null)
                {
                    TempData["ErrorMessage"] = "Novel not found.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                novel.IsActive = !novel.IsActive;
                novel.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Novel {(novel.IsActive ? "activated" : "deactivated")} successfully!";
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating status: " + ex.Message;
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModerateNovel(int id, string moderationAction, string moderationNotes = "")
        {
            try
            {
                var novel = db.Novels.Find(id);
                if (novel == null)
                {
                    TempData["ErrorMessage"] = "Novel not found.";
                    return RedirectToAction("Novel_Manager");
                }

                switch (moderationAction.ToLower())
                {
                    case "approve":
                        novel.ModerationStatus = "Approved";
                        break;
                    case "reject":
                        novel.ModerationStatus = "Rejected";
                        break;
                    default:
                        TempData["ErrorMessage"] = "Invalid moderation action.";
                        return RedirectToAction("Novel_Manager");
                }

                novel.ModerationDate = DateTime.Now;
                novel.ModeratedBy = 1;
                novel.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(moderationNotes))
                {
                    novel.ModerationNotes = moderationNotes;
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Novel {moderationAction}d successfully!";
                return RedirectToAction("Novel_Manager");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error during moderation: " + ex.Message;
                return RedirectToAction("Novel_Manager");
            }
        }

        [HttpGet]
        public ActionResult CreateNovel()
        {
            try
            {
                ViewBag.Genres = db.Genres.ToList();
                ViewBag.Tags = db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToList();
                return View(new Novel());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading create form: " + ex.Message;
                return RedirectToAction("Novel_Manager");
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


        #region Edit Novel Controller 
        // Add these methods to your Novel_ManagerController class

        [HttpGet]
        public ActionResult EditNovel(int id, string search = "", string statusFilter = "all",
            string moderationFilter = "all", string activeFilter = "all", int page = 1)
        {
            try
            {
                // Add debugging
                System.Diagnostics.Debug.WriteLine($"EditNovel GET called with id: {id}");

                // Validate id parameter
                if (id <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid ID: {id}");
                    TempData["ErrorMessage"] = "Invalid novel ID.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Load novel with related data - simplified approach
                var novel = db.Novels.FirstOrDefault(n => n.Id == id);

                if (novel == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Novel with ID {id} not found in database");
                    TempData["ErrorMessage"] = $"Novel with ID {id} not found.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Load author separately to avoid Include issues
                if (novel.AuthorId > 0)
                {
                    novel.Author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);
                    System.Diagnostics.Debug.WriteLine($"Author loaded: {novel.Author?.PenName ?? "No author"}");
                }

                // Load genres and tags separately
                var novelGenres = db.NovelGenres
                    .Where(ng => ng.NovelId == id)
                    .Select(ng => ng.GenreId)
                    .ToList();

                var novelTags = db.NovelTags
                    .Where(nt => nt.NovelId == id)
                    .Select(nt => nt.TagId)
                    .ToList();

                // Load all genres and tags for the form
                ViewBag.Genres = db.Genres.OrderBy(g => g.Name).ToList() ?? new List<Genre>();
                ViewBag.Tags = db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToList() ?? new List<Tag>();

                // Get currently selected genres and tags
                ViewBag.SelectedGenres = novelGenres;
                ViewBag.SelectedTags = novelTags;

                // Store return parameters for after update
                ViewBag.Search = search ?? "";
                ViewBag.StatusFilter = statusFilter ?? "all";
                ViewBag.ModerationFilter = moderationFilter ?? "all";
                ViewBag.ActiveFilter = activeFilter ?? "all";
                ViewBag.CurrentPage = page;

                // Ensure novel has default values for null properties
                if (novel.AlternativeTitle == null) novel.AlternativeTitle = "";
                if (novel.Synopsis == null) novel.Synopsis = "";
                if (novel.Status == null) novel.Status = "Ongoing";
                if (novel.Language == null) novel.Language = "English";
                if (novel.OriginalLanguage == null) novel.OriginalLanguage = "English";
                if (novel.TranslationStatus == null) novel.TranslationStatus = "Original";

                System.Diagnostics.Debug.WriteLine($"Successfully loaded novel: {novel.Title} (ID: {novel.Id})");
                return View(novel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EditNovel GET: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = "Error loading novel for editing: " + ex.Message;
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNovel(
            [Bind(Exclude = "CoverImage")] Novel model,
            string authorName,
            HttpPostedFileBase coverImage,
            int[] selectedGenres,
            int[] selectedTags,
            string search = "",
            string statusFilter = "all",
            string moderationFilter = "all",
            string activeFilter = "all",
            int page = 1)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EditNovel POST called with id: {model.Id}");

                // Validate input
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    TempData["ErrorMessage"] = "Novel title is required.";
                    return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                }

                if (string.IsNullOrWhiteSpace(authorName))
                {
                    TempData["ErrorMessage"] = "Author name is required.";
                    return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Get the existing novel from database - SIMPLIFIED QUERY
                var existingNovel = db.Novels.FirstOrDefault(n => n.Id == model.Id);

                if (existingNovel == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Novel with ID {model.Id} not found in database");
                    TempData["ErrorMessage"] = "Novel not found.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                System.Diagnostics.Debug.WriteLine($"Found existing novel: {existingNovel.Title}");

                // Check for duplicate title (exclude current novel)
                if (db.Novels.Any(n => n.Id != model.Id && n.Title.ToLower() == model.Title.ToLower()))
                {
                    TempData["ErrorMessage"] = "A novel with this title already exists.";
                    return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Handle author change/update - LOAD SEPARATELY
                var currentAuthor = db.Authors.FirstOrDefault(a => a.Id == existingNovel.AuthorId);
                if (currentAuthor == null || currentAuthor.PenName != authorName.Trim())
                {
                    // Find or create new author
                    var newAuthor = db.Authors.FirstOrDefault(a => a.PenName.ToLower() == authorName.ToLower());
                    if (newAuthor == null)
                    {
                        // Create new author similar to CreateNovel logic
                        string baseUsername = authorName.Trim().Replace(" ", "").ToLower();
                        string baseEmail = $"{baseUsername}@example.com";

                        string uniqueUsername = baseUsername;
                        int counter = 1;
                        while (db.Users.Any(u => u.Username == uniqueUsername))
                        {
                            uniqueUsername = $"{baseUsername}{counter}";
                            counter++;
                        }

                        string uniqueEmail = baseEmail;
                        counter = 1;
                        while (db.Users.Any(u => u.Email == uniqueEmail))
                        {
                            uniqueEmail = $"{baseUsername}{counter}@example.com";
                            counter++;
                        }

                        var newUser = new User
                        {
                            Username = uniqueUsername,
                            Email = uniqueEmail,
                            PasswordHash = "password123",
                            FirstName = authorName.Split(' ').FirstOrDefault() ?? authorName.Trim(),
                            LastName = authorName.Split(' ').Length > 1 ? string.Join(" ", authorName.Split(' ').Skip(1)) : "",
                            JoinDate = DateTime.Now,
                            IsActive = true,
                            EmailVerified = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        newAuthor = new Author
                        {
                            UserId = newUser.Id,
                            PenName = authorName.Trim(),
                            Biography = $"Author profile for {authorName.Trim()}",
                            IsVerified = false,
                            AuthorRank = "Novice",
                            TotalNovels = 0,
                            TotalViews = 0,
                            TotalFollowers = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        db.Authors.Add(newAuthor);
                        db.SaveChanges();

                        var authorRole = db.UserRoles.FirstOrDefault(r => r.Name == "Author");
                        if (authorRole != null)
                        {
                            db.UserRoleAssignments.Add(new UserRoleAssignment
                            {
                                UserId = newUser.Id,
                                RoleId = authorRole.Id,
                                AssignedDate = DateTime.Now,
                                IsActive = true
                            });
                            db.SaveChanges();
                        }
                    }
                    existingNovel.AuthorId = newAuthor.Id;
                }

                // Update novel properties
                existingNovel.Title = model.Title.Trim();
                existingNovel.AlternativeTitle = model.AlternativeTitle?.Trim();
                existingNovel.Synopsis = model.Synopsis?.Trim();
                existingNovel.Status = model.Status ?? existingNovel.Status;
                existingNovel.Language = model.Language ?? existingNovel.Language;
                existingNovel.OriginalLanguage = model.OriginalLanguage ?? existingNovel.OriginalLanguage;
                existingNovel.TranslationStatus = model.TranslationStatus ?? existingNovel.TranslationStatus;
                existingNovel.IsActive = model.IsActive;
                existingNovel.IsPremium = model.IsPremium;
                existingNovel.IsOriginal = model.IsOriginal;
                existingNovel.IsFeatured = model.IsFeatured;
                existingNovel.IsWeeklyFeatured = model.IsWeeklyFeatured;
                existingNovel.IsSliderFeatured = model.IsSliderFeatured;
                existingNovel.UpdatedAt = DateTime.Now;

                // Handle cover image update INLINE (avoid separate method call)
                if (coverImage != null && coverImage.ContentLength > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing new cover image: {coverImage.FileName}");

                    if (coverImage.ContentLength > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Cover image file size cannot exceed 5MB.";
                        return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                    }

                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(coverImage.ContentType.ToLower()))
                    {
                        TempData["ErrorMessage"] = "Only JPEG, PNG, GIF, and WebP image formats are allowed.";
                        return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                    }

                    try
                    {
                        using (var binaryReader = new System.IO.BinaryReader(coverImage.InputStream))
                        {
                            existingNovel.CoverImage = binaryReader.ReadBytes(coverImage.ContentLength);
                            existingNovel.CoverImageContentType = coverImage.ContentType;
                            existingNovel.CoverImageFileName = coverImage.FileName;
                        }
                        System.Diagnostics.Debug.WriteLine("Cover image processed successfully");
                    }
                    catch (Exception imgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing cover image: {imgEx.Message}");
                        TempData["ErrorMessage"] = "Error processing cover image: " + imgEx.Message;
                        return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
                    }
                }

                // Update genres - HANDLE SEPARATELY
                try
                {
                    System.Diagnostics.Debug.WriteLine("Updating genres...");

                    // Remove existing genres
                    var existingGenres = db.NovelGenres.Where(ng => ng.NovelId == existingNovel.Id).ToList();
                    if (existingGenres.Any())
                    {
                        db.NovelGenres.RemoveRange(existingGenres);
                    }

                    // Add new genres
                    if (selectedGenres != null && selectedGenres.Length > 0)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            db.NovelGenres.Add(new NovelGenre
                            {
                                NovelId = existingNovel.Id,
                                GenreId = genreId
                            });
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Added {selectedGenres?.Length ?? 0} genres");
                }
                catch (Exception genreEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating genres: {genreEx.Message}");
                    // Continue with tags update, don't fail completely
                }

                // Update tags - HANDLE SEPARATELY
                try
                {
                    System.Diagnostics.Debug.WriteLine("Updating tags...");

                    // Remove existing tags
                    var existingTags = db.NovelTags.Where(nt => nt.NovelId == existingNovel.Id).ToList();
                    if (existingTags.Any())
                    {
                        db.NovelTags.RemoveRange(existingTags);
                    }

                    // Add new tags
                    if (selectedTags != null && selectedTags.Length > 0)
                    {
                        foreach (var tagId in selectedTags)
                        {
                            db.NovelTags.Add(new NovelTag
                            {
                                NovelId = existingNovel.Id,
                                TagId = tagId,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Added {selectedTags?.Length ?? 0} tags");
                }
                catch (Exception tagEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating tags: {tagEx.Message}");
                    // Continue with save, don't fail completely
                }

                // SINGLE SAVE CHANGES CALL
                System.Diagnostics.Debug.WriteLine("Saving all changes...");
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Save completed successfully");

                TempData["SuccessMessage"] = $"Novel '{existingNovel.Title}' updated successfully!";
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EditNovel POST: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }

                var inner = ex.InnerException?.Message ?? "";
                var inner2 = ex.InnerException?.InnerException?.Message ?? "";
                TempData["ErrorMessage"] = $"Error updating novel: {ex.Message} | Inner: {inner} | Inner2: {inner2}";
                return RedirectToAction("EditNovel", new { id = model.Id, search, statusFilter, moderationFilter, activeFilter, page });
            }
        }


        #endregion

        // Optional: Add a method to get novel details for AJAX calls or API
        [HttpGet]
        public JsonResult GetNovelDetails(int id)
        {
            try
            {
                var novel = db.Novels
                    .Include(n => n.Author)
                    .Include(n => n.NovelGenres.Select(ng => ng.Genre))
                    .Include(n => n.NovelTags.Select(nt => nt.Tag))
                    .FirstOrDefault(n => n.Id == id);

                if (novel == null)
                {
                    return Json(new { success = false, message = "Novel not found" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    novel = new
                    {
                        id = novel.Id,
                        title = novel.Title,
                        alternativeTitle = novel.AlternativeTitle,
                        synopsis = novel.Synopsis,
                        status = novel.Status,
                        author = novel.Author?.PenName,
                        language = novel.Language,
                        originalLanguage = novel.OriginalLanguage,
                        translationStatus = novel.TranslationStatus,
                        isActive = novel.IsActive,
                        isPremium = novel.IsPremium,
                        isOriginal = novel.IsOriginal,
                        isFeatured = novel.IsFeatured,
                        isWeeklyFeatured = novel.IsWeeklyFeatured,
                        isSliderFeatured = novel.IsSliderFeatured,
                        genres = novel.NovelGenres?.Select(ng => new { id = ng.Genre.Id, name = ng.Genre.Name }).ToList(),
                        tags = novel.NovelTags?.Select(nt => new { id = nt.Tag.Id, name = nt.Tag.DisplayName }).ToList(),
                        hasCoverImage = novel.CoverImage != null && novel.CoverImage.Length > 0,
                        coverImageUrl = novel.CoverImage != null && novel.CoverImage.Length > 0 ? Url.Action("GetCoverImage", new { id = novel.Id }) : null
                    }
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to validate novel data (can be used in both Create and Edit)
        private bool ValidateNovelData(Novel novel, string authorName, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(novel.Title))
            {
                errorMessage = "Novel title is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                errorMessage = "Author name is required.";
                return false;
            }

            if (novel.Title.Length > 200)
            {
                errorMessage = "Novel title cannot exceed 200 characters.";
                return false;
            }

            if (!string.IsNullOrEmpty(novel.AlternativeTitle) && novel.AlternativeTitle.Length > 200)
            {
                errorMessage = "Alternative title cannot exceed 200 characters.";
                return false;
            }

            if (!string.IsNullOrEmpty(novel.Synopsis) && novel.Synopsis.Length > 5000)
            {
                errorMessage = "Synopsis cannot exceed 5000 characters.";
                return false;
            }

            return true;
        }



















        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNovel(
            [Bind(Exclude = "CoverImage")] Novel model, // <-- ignore binary binding
            string authorName,
            HttpPostedFileBase coverImage,
            int[] selectedGenres,
            int[] selectedTags,
            string search = "",
            string statusFilter = "all",  
            string moderationFilter = "all",
            string activeFilter = "all",
            int page = 1)
        {
            try
            {
                // Validate title
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    TempData["ErrorMessage"] = "Novel title is required.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Validate author
                if (string.IsNullOrWhiteSpace(authorName))
                {
                    TempData["ErrorMessage"] = "Author name is required.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Check duplicate title
                if (db.Novels.Any(n => n.Title.ToLower() == model.Title.ToLower()))
                {
                    TempData["ErrorMessage"] = "A novel with this title already exists.";
                    return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
                }

                // Find or create author (FIXED VERSION)
                var author = db.Authors.FirstOrDefault(a => a.PenName.ToLower() == authorName.ToLower());
                if (author == null)
                {
                    // Generate a unique username and email
                    string baseUsername = authorName.Trim().Replace(" ", "").ToLower();
                    string baseEmail = $"{baseUsername}@example.com";

                    // Check if username already exists and make it unique
                    string uniqueUsername = baseUsername;
                    int counter = 1;
                    while (db.Users.Any(u => u.Username == uniqueUsername))
                    {
                        uniqueUsername = $"{baseUsername}{counter}";
                        counter++;
                    }

                    // Check if email already exists and make it unique
                    string uniqueEmail = baseEmail;
                    counter = 1;
                    while (db.Users.Any(u => u.Email == uniqueEmail))
                    {
                        uniqueEmail = $"{baseUsername}{counter}@example.com";
                        counter++;
                    }

                    // Create user with unique username and email
                    var newUser = new User
                    {
                        Username = uniqueUsername,
                        Email = uniqueEmail,
                        PasswordHash = "password123",
                        FirstName = authorName.Split(' ').FirstOrDefault() ?? authorName.Trim(),
                        LastName = authorName.Split(' ').Length > 1 ? string.Join(" ", authorName.Split(' ').Skip(1)) : "",
                        JoinDate = DateTime.Now,
                        IsActive = true,
                        EmailVerified = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges(); // newUser.Id is now populated

                    // Create author (no Id assigned manually)
                    var newAuthor = new Author
                    {
                        UserId = newUser.Id,
                        PenName = authorName.Trim(),
                        Biography = $"Author profile for {authorName.Trim()}",
                        IsVerified = false,
                        AuthorRank = "Novice",
                        TotalNovels = 0,
                        TotalViews = 0,
                        TotalFollowers = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.Authors.Add(newAuthor);
                    db.SaveChanges(); // newAuthor.Id now populated

                    author = newAuthor;

                    var authorRole = db.UserRoles.FirstOrDefault(r => r.Name == "Author");
                    if (authorRole != null)
                    {
                        db.UserRoleAssignments.Add(new UserRoleAssignment
                        {
                            UserId = newUser.Id,
                            RoleId = authorRole.Id,
                            AssignedDate = DateTime.Now,
                            IsActive = true
                        });
                        db.SaveChanges();
                    }
                }

                // Create novel (CoverImage excluded here)
                var novel = new Novel
                {
                    Title = model.Title.Trim(),
                    AlternativeTitle = model.AlternativeTitle?.Trim(),
                    Synopsis = model.Synopsis?.Trim(),
                    Status = model.Status ?? "Ongoing",
                    AuthorId = author.Id,
                    Language = model.Language ?? "EN",
                    OriginalLanguage = model.OriginalLanguage,
                    TranslationStatus = model.TranslationStatus ?? "Original",
                    IsActive = model.IsActive,
                    IsPremium = model.IsPremium,
                    IsOriginal = model.IsOriginal,
                    IsFeatured = model.IsFeatured,
                    IsWeeklyFeatured = model.IsWeeklyFeatured,
                    IsSliderFeatured = model.IsSliderFeatured,
                    ModerationStatus = "Pending",
                    CreatedAt = DateTime.Now
                };

                // Save cover image as binary
                SaveCoverImageToDatabase(novel, coverImage);

                db.Novels.Add(novel);
                db.SaveChanges();

                // Genres
                if (selectedGenres != null && selectedGenres.Length > 0)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        db.NovelGenres.Add(new NovelGenre
                        {
                            NovelId = novel.Id,
                            GenreId = genreId
                        });
                    }
                }

                // Tags
                if (selectedTags != null && selectedTags.Length > 0)
                {
                    foreach (var tagId in selectedTags)
                    {
                        db.NovelTags.Add(new NovelTag
                        {
                            NovelId = novel.Id,
                            TagId = tagId,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Novel created successfully!";
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                var inner2 = ex.InnerException?.InnerException?.Message ?? "";
                TempData["ErrorMessage"] = $"Error creating novel: {ex.Message} | Inner: {inner} | Inner2: {inner2}";
                return RedirectToAction("Novel_Manager", new { search, statusFilter, moderationFilter, activeFilter, page });
            }
        }


        private string SaveCoverImage(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var uploadPath = Server.MapPath("~/Content/uploads/covers/");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    file.SaveAs(filePath);

                    return "/Content/uploads/covers/" + fileName;
                }
            }
            catch (Exception)
            {
                // Log error if needed
            }
            return null;
        }

        private void SaveCoverImageToDatabase(Novel novel, HttpPostedFileBase coverImage)
        {
            if (coverImage != null && coverImage.ContentLength > 0)
            {
                if (coverImage.ContentLength > 5 * 1024 * 1024)
                {
                    throw new Exception("Cover image file size cannot exceed 5MB.");
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(coverImage.ContentType.ToLower()))
                {
                    throw new Exception("Only JPEG, PNG, GIF, and WebP image formats are allowed.");
                }

                using (var binaryReader = new System.IO.BinaryReader(coverImage.InputStream))
                {
                    novel.CoverImage = binaryReader.ReadBytes(coverImage.ContentLength);
                    novel.CoverImageContentType = coverImage.ContentType;
                    novel.CoverImageFileName = coverImage.FileName;
                }
            }
        }

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

                // Return a default "no image" placeholder
                string defaultImagePath = Server.MapPath("~/Content/images/no-cover-placeholder.jpg");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    return File(defaultImagePath, "image/jpeg");
                }

                return new HttpStatusCodeResult(404);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500);
            }
        }   

















    }
}