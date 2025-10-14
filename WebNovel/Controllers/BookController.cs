using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Controllers
{
    public class BookController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        [HttpGet]
        public ActionResult BookDetail(int? id, int? loadAll = null, int? goToChapter = null, string sort = "latest", int page = 1, int pageSize = 10)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                if (goToChapter.HasValue)
                {
                    var chapterExists = db.Chapters.Any(c => c.NovelId == id.Value && c.ChapterNumber == goToChapter.Value);
                    if (chapterExists)
                    {
                        return RedirectToAction("ReadChapter", new { novelId = id.Value, chapterNumber = goToChapter.Value });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Chapter {goToChapter.Value} not found.";
                    }
                }

                int? currentUserId = GetCurrentUserId();
                ViewBag.IsLoggedIn = currentUserId.HasValue;

                // Calculate pagination for comments
                int skip = (page - 1) * pageSize;
                int totalComments = GetTotalCommentsCount(id.Value);
                int totalPages = (int)Math.Ceiling((double)totalComments / pageSize);

                // Set pagination ViewBag values
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalComments = totalComments;

                var novel = GetNovelDetails(id.Value, loadAll.HasValue && loadAll.Value == 1, sort, skip, pageSize);
                if (novel == null)
                {
                    return HttpNotFound();
                }

                if (!loadAll.HasValue && !goToChapter.HasValue)
                {
                    UpdateViewCount(id.Value);
                }

                ViewBag.ShowAllChapters = loadAll.HasValue && loadAll.Value == 1;
                ViewBag.SortOrder = sort;



                ViewBag.RandomNovels = GetRandomNovelsForDisplay(id.Value, 5, Url);

                return View(novel);
            }
            catch (Exception ex)
            {
                // Thêm log lỗi ở đây để dễ debug hơn
                // Log.Error(ex, "An error occurred in BookDetail action.");
                throw;
            }
        }
        private List<NovelCardViewModel> GetRandomNovelsForDisplay(int currentNovelId, int count, UrlHelper url)
        {
            // Bước 1: Lấy các đối tượng Novel ngẫu nhiên trực tiếp từ DB
            var randomNovelEntities = db.Novels
                .Include(n => n.Author)
                .Include(n => n.NovelGenres.Select(ng => ng.Genre))
                .Where(n => n.Id != currentNovelId)
                // ⭐ THAY ĐỔI CHÍNH LÀ Ở ĐÂY ⭐
                // Yêu cầu DB sắp xếp kết quả một cách ngẫu nhiên
                .OrderBy(n => Guid.NewGuid())
                .Take(count)
                .ToList();

            // Bước 2: Dùng Select để chiếu sang ViewModel và tạo URL ảnh (giữ nguyên)
            var result = randomNovelEntities.Select(n => new NovelCardViewModel
            {
                Id = n.Id,
                Slug = n.Slug,
                Title = n.Title,
                Status = n.Status,
                AverageRating = n.AverageRating,
                TotalChapters = n.TotalChapters,
                AuthorName = n.Author?.PenName,
                GenreName = n.NovelGenres?.Select(ng => ng.Genre.Name).FirstOrDefault() ?? "Unknown", // Sửa thành Unknown
                BookmarkCount = n.BookmarkCount,
                CoverImageUrl = n.HasCoverImage ? url.Action("GetCoverImage", "Book", new { id = n.Id }) : null
            }).ToList();

            return result;
        }
   
        private NovelDetailsViewModel GetNovelDetails(int novelId, bool loadAllChapters = false, string sortOrder = "latest", int commentSkip = 0, int commentTake = 10)
        {
            Debug.WriteLine($"[DEBUG] GetNovelDetails called for novel ID: {novelId}, loadAllChapters: {loadAllChapters}, sortOrder: {sortOrder}, commentSkip: {commentSkip}, commentTake: {commentTake}");
            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == novelId && n.IsActive);
                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] No active novel found with ID: {novelId}");
                    return null;
                }
                Debug.WriteLine($"[DEBUG] Found novel: {novel.Title}, IsActive: {novel.IsActive}");
                LoadNovelRelatedData(novel);
                if (novel.Author == null)
                {
                    Debug.WriteLine("[ERROR] Failed to load Author or Author is null");
                    return null;
                }

                Debug.WriteLine($"[DEBUG] Author loaded: {novel.Author.PenName}");

                // Get current user ID
                int? currentUserId = GetCurrentUserId();
                Debug.WriteLine($"[DEBUG] Current user ID: {currentUserId}");

                Debug.WriteLine("[DEBUG] Creating NovelDetailsViewModel");
                var viewModel = new NovelDetailsViewModel
                {
                    Id = novel.Id,
                    Title = novel.Title,
                    Slug = novel.Slug ?? SlugHelper.GenerateSlug(novel.Title),
                    AlternativeTitle = novel.AlternativeTitle,
                    Synopsis = novel.Synopsis,
                    CoverImageUrl = novel.CoverImageUrl,
                    Status = novel.Status,
                    PublishDate = novel.PublishDate,
                    LastUpdated = novel.LastUpdated,
                    Language = novel.Language,
                    OriginalLanguage = novel.OriginalLanguage,
                    IsOriginal = novel.IsOriginal,
                    ViewCount = novel.ViewCount,
                    BookmarkCount = novel.BookmarkCount,
                    AverageRating = novel.AverageRating,
                    TotalRatings = novel.TotalRatings,
                    TotalChapters = novel.TotalChapters,
                    WordCount = novel.WordCount,
                    IsPremium = novel.IsPremium,

                    Author = new AuthorViewModel
                    {
                        Id = novel.Author.Id,
                        UserId = novel.Author.UserId,
                        PenName = novel.Author.PenName,
                        Biography = novel.Author.Biography,
                        Website = novel.Author.Website,
                        SocialLinks = novel.Author.SocialLinks,
                        IsVerified = novel.Author.IsVerified,
                        VerificationDate = novel.Author.VerificationDate,
                        AuthorRank = novel.Author.AuthorRank,
                        TotalNovels = novel.Author.TotalNovels,
                        TotalViews = novel.Author.TotalViews,
                        TotalFollowers = novel.Author.TotalFollowers,
                        Username = novel.Author.User?.Username,
                        Email = novel.Author.User?.Email,
                        FullName = !string.IsNullOrEmpty(novel.Author.User?.FirstName) && !string.IsNullOrEmpty(novel.Author.User?.LastName)
                            ? $"{novel.Author.User.FirstName} {novel.Author.User.LastName}"
                            : novel.Author.User?.Username
                    },

                    CurrentRank = GetNovelRank(novelId),
                    Genres = novel.NovelGenres?.Select(ng => ng.Genre?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                    Tags = novel.NovelTags?.Select(nt => nt.Tag?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),

                    // Updated to load chapters with sorting
                    RecentChapters = GetChapters(novelId, loadAllChapters, sortOrder),
                    Reviews = GetReviews(novelId, 10, 0),

                    // FIXED: Pass the pagination parameters to GetComments
                    Comments = GetComments(novelId, commentTake, commentSkip),

                    // User-specific data
                    IsBookmarked = IsNovelBookmarked(novelId, currentUserId),
                    UserRating = GetUserRating(novelId, currentUserId),
                    LastReadChapter = GetLastReadChapter(novelId, currentUserId)
                };

                Debug.WriteLine($"[DEBUG] ViewModel created successfully for novel: {viewModel.Title}");
                return viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in GetNovelDetails: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpGet]
        public ActionResult BookDetailBySlug(string slug)
        {
            Debug.WriteLine($"[DEBUG] BookDetailBySlug action called with slug: {slug}");

            if (string.IsNullOrEmpty(slug))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Slug == slug && n.IsActive);
                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] Novel with slug '{slug}' not found");
                    return HttpNotFound();
                }

                // Redirect to the existing BookDetail method with the ID
                return BookDetail(novel.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in BookDetailBySlug: {ex.Message}");
                throw;
            }
        }


        [HttpGet]
        public ActionResult GetCoverImage(int id)
        {
            Debug.WriteLine($"[DEBUG] GetCoverImage called for ID: {id}");

            try
            {
                var novel = db.Novels.Find(id);
                Debug.WriteLine($"[DEBUG] Novel found: {novel != null}");

                if (novel != null)
                {
                    Debug.WriteLine($"[DEBUG] Novel title: {novel.Title}");
                    Debug.WriteLine($"[DEBUG] CoverImage is null: {novel.CoverImage == null}");

                    if (novel.CoverImage != null)
                    {
                        Debug.WriteLine($"[DEBUG] CoverImage length: {novel.CoverImage.Length}");
                    }
                }

                if (novel?.CoverImage != null && novel.CoverImage.Length > 0)
                {
                    string contentType = novel.CoverImageContentType ?? "image/jpeg";
                    Debug.WriteLine($"[DEBUG] Returning cover image with content type: {contentType}");
                    return File(novel.CoverImage, contentType);
                }

                // Return default image
                Debug.WriteLine($"[DEBUG] No cover image found, trying default image");
                string defaultImagePath = Server.MapPath("~/Content/images/no-cover-placeholder.jpg");

                if (System.IO.File.Exists(defaultImagePath))
                {
                    Debug.WriteLine($"[DEBUG] Default image found at: {defaultImagePath}");
                    return File(defaultImagePath, "image/jpeg");
                }

                Debug.WriteLine($"[DEBUG] Default image not found, returning 404");
                return new HttpStatusCodeResult(404);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in GetCoverImage for ID {id}: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return new HttpStatusCodeResult(500);
            }
        }


        // Add this diagnostic method to your BookController
        [HttpGet]
        public JsonResult DiagnoseImages()
        {
            try
            {
                var imageStats = db.Novels
                    .Select(n => new
                    {
                        Id = n.Id,
                        Title = n.Title,
                        HasCoverImage = n.CoverImage != null,
                        ImageSize = n.CoverImage != null ? n.CoverImage.Length : 0,
                        ContentType = n.CoverImageContentType,
                        CoverImageUrl = n.CoverImageUrl
                    })
                    .OrderBy(n => n.Id)
                    .ToList();

                var diagnostics = new
                {
                    TotalNovels = imageStats.Count,
                    NovelsWithImages = imageStats.Count(i => i.HasCoverImage),
                    NovelsWithoutImages = imageStats.Count(i => !i.HasCoverImage),
                    AverageImageSize = imageStats.Where(i => i.HasCoverImage).Average(i => (double?)i.ImageSize) ?? 0,
                    ImageDetails = imageStats.Take(20) // Show first 20 for debugging
                };

                Debug.WriteLine($"[DIAGNOSTIC] Total novels: {diagnostics.TotalNovels}");
                Debug.WriteLine($"[DIAGNOSTIC] With images: {diagnostics.NovelsWithImages}");
                Debug.WriteLine($"[DIAGNOSTIC] Without images: {diagnostics.NovelsWithoutImages}");
                Debug.WriteLine($"[DIAGNOSTIC] Average image size: {diagnostics.AverageImageSize:F0} bytes");

                return Json(diagnostics, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Diagnostic failed: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to detect image type from byte array
        private string DetectImageType(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 4)
                return "unknown";

            // Check for common image file signatures
            // JPEG
            if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return "jpeg";

            // PNG
            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                return "png";

            // GIF
            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
                return "gif";

            // BMP
            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return "bmp";

            // WebP
            if (imageData.Length >= 12 &&
                imageData[0] == 0x52 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x46 &&
                imageData[8] == 0x57 && imageData[9] == 0x45 && imageData[10] == 0x42 && imageData[11] == 0x50)
                return "webp";

            Debug.WriteLine($"[DEBUG] Unknown image format. First 8 bytes: {BitConverter.ToString(imageData.Take(8).ToArray())}");
            return "unknown";
        }

        // Helper method to get content type from detected image type
        private string GetContentTypeFromImageType(string imageType)
        {
            switch (imageType.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "image/bmp";
                case "webp":
                    return "image/webp";
                default:
                    return "image/jpeg"; // Default fallback
            }
        }

        private NovelDetailsViewModel GetNovelDetails(int novelId, bool loadAllChapters = false, string sortOrder = "latest")
        {
            Debug.WriteLine($"[DEBUG] GetNovelDetails called for novel ID: {novelId}, loadAllChapters: {loadAllChapters}, sortOrder: {sortOrder}");
            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == novelId && n.IsActive);
                if (novel == null)
                {
                    Debug.WriteLine($"[DEBUG] No active novel found with ID: {novelId}");
                    return null;
                }
                Debug.WriteLine($"[DEBUG] Found novel: {novel.Title}, IsActive: {novel.IsActive}");
                LoadNovelRelatedData(novel);
                if (novel.Author == null)
                {
                    Debug.WriteLine("[ERROR] Failed to load Author or Author is null");
                    return null;
                }

                Debug.WriteLine($"[DEBUG] Author loaded: {novel.Author.PenName}");

                // Get current user ID
                int? currentUserId = GetCurrentUserId();
                Debug.WriteLine($"[DEBUG] Current user ID: {currentUserId}");

                Debug.WriteLine("[DEBUG] Creating NovelDetailsViewModel");
                var viewModel = new NovelDetailsViewModel
                {
                    Id = novel.Id,
                    Title = novel.Title,
                    Slug = novel.Slug ?? SlugHelper.GenerateSlug(novel.Title),
                    AlternativeTitle = novel.AlternativeTitle,
                    Synopsis = novel.Synopsis,
                    CoverImageUrl = novel.CoverImageUrl,
                    Status = novel.Status,
                    PublishDate = novel.PublishDate,
                    LastUpdated = novel.LastUpdated,
                    Language = novel.Language,
                    OriginalLanguage = novel.OriginalLanguage,
                    IsOriginal = novel.IsOriginal,
                    ViewCount = novel.ViewCount,
                    BookmarkCount = novel.BookmarkCount,
                    AverageRating = novel.AverageRating,
                    TotalRatings = novel.TotalRatings,
                    TotalChapters = novel.TotalChapters,
                    WordCount = novel.WordCount,
                    IsPremium = novel.IsPremium,

                    Author = new AuthorViewModel
                    {
                        Id = novel.Author.Id,
                        UserId = novel.Author.UserId,
                        PenName = novel.Author.PenName,
                        Biography = novel.Author.Biography,
                        Website = novel.Author.Website,
                        SocialLinks = novel.Author.SocialLinks,
                        IsVerified = novel.Author.IsVerified,
                        VerificationDate = novel.Author.VerificationDate,
                        AuthorRank = novel.Author.AuthorRank,
                        TotalNovels = novel.Author.TotalNovels,
                        TotalViews = novel.Author.TotalViews,
                        TotalFollowers = novel.Author.TotalFollowers,
                        Username = novel.Author.User?.Username,
                        Email = novel.Author.User?.Email,
                        FullName = !string.IsNullOrEmpty(novel.Author.User?.FirstName) && !string.IsNullOrEmpty(novel.Author.User?.LastName)
                            ? $"{novel.Author.User.FirstName} {novel.Author.User.LastName}"
                            : novel.Author.User?.Username
                    },

                    CurrentRank = GetNovelRank(novelId),
                    Genres = novel.NovelGenres?.Select(ng => ng.Genre?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                    Tags = novel.NovelTags?.Select(nt => nt.Tag?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),

                    // Updated to load chapters with sorting
                    RecentChapters = GetChapters(novelId, loadAllChapters, sortOrder),
                    Reviews = GetReviews(novelId, 10, 0),
                    Comments = GetComments(novelId),

                    // User-specific data
                    IsBookmarked = IsNovelBookmarked(novelId, currentUserId),
                    UserRating = GetUserRating(novelId, currentUserId),
                    LastReadChapter = GetLastReadChapter(novelId, currentUserId)
                };

                Debug.WriteLine($"[DEBUG] ViewModel created successfully for novel: {viewModel.Title}");
                return viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in GetNovelDetails: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private List<ChapterViewModel> GetChapters(int novelId, bool loadAll = false, string sortOrder = "latest")
        {
            Debug.WriteLine($"[DEBUG] Getting chapters for novel ID: {novelId}, loadAll: {loadAll}, sortOrder: {sortOrder}");

            var baseQuery = db.Chapters.Where(c => c.NovelId == novelId);

            // Apply sorting
            switch (sortOrder.ToLower())
            {
                case "oldest":
                    baseQuery = baseQuery.OrderBy(c => c.ChapterNumber);
                    break;
                case "number":
                    baseQuery = baseQuery.OrderBy(c => c.ChapterNumber);
                    break;
                case "latest":
                default:
                    baseQuery = baseQuery.OrderByDescending(c => c.ChapterNumber);
                    break;
            }

            // Load all chapters for JavaScript to handle showing/hiding
            var chapters = baseQuery
                .Select(c => new
                {
                    Id = c.Id,
                    ChapterNumber = c.ChapterNumber,
                    Title = c.Title,
                    PublishDate = c.PublishDate,
                    ViewCount = c.ViewCount,
                    IsPremium = c.IsPremium,
                    CoinCost = c.UnlockPrice
                })
                .ToList();

            int? currentUserId = GetCurrentUserId();
            return chapters.Select(c => new ChapterViewModel
            {
                Id = c.Id,
                ChapterNumber = c.ChapterNumber,
                Title = c.Title,
                PublishDate = c.PublishDate,
                ViewCount = c.ViewCount,
                IsPremium = c.IsPremium,
                CoinCost = c.CoinCost,
                IsUnlocked = !c.IsPremium || IsChapterUnlocked(c.Id, currentUserId)
            }).ToList();
        }

        public ActionResult ReadChapter(int novelId, int chapterNumber)
        {
            Debug.WriteLine($"[DEBUG] ReadChapter called for novel ID: {novelId}, chapter: {chapterNumber}");

            var chapter = db.Chapters.FirstOrDefault(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = $"Chapter {chapterNumber} not found.";
                return RedirectToAction("BookDetail", new { id = novelId });
            }

            // Redirect to your chapter reading page/action
            // Adjust this based on your existing chapter reading implementation
            return RedirectToAction("Chapter", "Reader", new { id = chapter.Id });
        }

        
        private void LoadNovelRelatedData(Novel novel)
        {
            try
            {
                // Load Author
                if (novel.AuthorId != 0)
                {
                    novel.Author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);

                    // Load Author's User if available
                    if (novel.Author != null && novel.Author.UserId != 0)
                    {
                        novel.Author.User = db.Users.FirstOrDefault(u => u.Id == novel.Author.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Author: {ex.Message}");
            }

            try
            {
                // Load Genres
                var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                novel.NovelGenres = novelGenres;

                foreach (var ng in novelGenres)
                {
                    ng.Genre = db.Genres.FirstOrDefault(g => g.Id == ng.GenreId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Genres: {ex.Message}");
                novel.NovelGenres = new List<NovelGenre>();
            }

            try
            {
                // Load Tags
                var novelTags = db.NovelTags.Where(nt => nt.NovelId == novel.Id).ToList();
                novel.NovelTags = novelTags;

                foreach (var nt in novelTags)
                {
                    nt.Tag = db.Tags.FirstOrDefault(t => t.Id == nt.TagId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Tags: {ex.Message}");
                novel.NovelTags = new List<NovelTag>();
            }

            try
            {
                // Load Chapters (if needed for the view model)
                novel.Chapters = db.Chapters.Where(c => c.NovelId == novel.Id).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading Chapters: {ex.Message}");
                novel.Chapters = new List<Chapter>();
            }
        }

        private List<ChapterViewModel> GetRecentChapters(int novelId, int take = 5)
        {
            Debug.WriteLine($"[DEBUG] Getting recent chapters for novel ID: {novelId}");

            var chapters = db.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderByDescending(c => c.ChapterNumber)
                .Take(take)
                .Select(c => new
                {
                    Id = c.Id,
                    ChapterNumber = c.ChapterNumber,
                    Title = c.Title,
                    PublishDate = c.PublishDate,
                    ViewCount = c.ViewCount,
                    IsPremium = c.IsPremium,
                    CoinCost = c.UnlockPrice
                })
                .ToList(); // Execute the query first

            // Then apply the custom method in memory
            int? currentUserId = GetCurrentUserId();
            return chapters.Select(c => new ChapterViewModel
            {
                Id = c.Id,
                ChapterNumber = c.ChapterNumber,
                Title = c.Title,
                PublishDate = c.PublishDate,
                ViewCount = c.ViewCount,
                IsPremium = c.IsPremium,
                CoinCost = c.CoinCost,
                IsUnlocked = !c.IsPremium || IsChapterUnlocked(c.Id, currentUserId)
            }).ToList();
        }

        private List<ReviewViewModel> GetReviews(int novelId, int take = 10)
        {
            Debug.WriteLine($"[DEBUG] Getting reviews for novel ID: {novelId}");
            // Use a different approach to avoid Join complexity
            var commentsWithRatings = db.Comments
                .Where(c => c.NovelId == novelId && c.IsApproved && c.ParentCommentId == null)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Take(take)
                .ToList()
                .Select(c => new
                {
                    Comment = c,
                    Rating = db.Ratings.FirstOrDefault(r => r.ReaderId == c.UserId && r.NovelId == c.NovelId)
                })
                .Where(cr => cr.Rating != null) // Only include comments that have ratings
                .Select(cr => new ReviewViewModel
                {
                    Id = cr.Comment.Id,
                    ReviewerName = cr.Comment.User.Username,
                    ReviewerInitials =  GetUserInitials(cr.Comment.User.Username),
                    Title = ExtractReviewTitle(cr.Comment.Content),
                    Content = cr.Comment.Content,
                    Rating = cr.Rating.RatingValue,
                    CreatedDate = cr.Comment.CreatedAt,
                    LikeCount = cr.Comment.LikeCount,
                    DislikeCount = cr.Comment.DislikeCount
                })
                .ToList();

            return commentsWithRatings;
        }






































        #region Comment System Methods - Enhanced for Nested Comments


        private void SaveCommentImageToDatabase(Comment comment, HttpPostedFileBase commentImage)
        {
            if (commentImage != null && commentImage.ContentLength > 0)
            {
                // Validate file size (limit to 2MB for comments)
                if (commentImage.ContentLength > 2 * 1024 * 1024)
                {
                    throw new Exception("Comment image file size cannot exceed 2MB.");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(commentImage.ContentType.ToLower()))
                {
                    throw new Exception("Only JPEG, PNG, GIF, and WebP image formats are allowed.");
                }

                using (var binaryReader = new System.IO.BinaryReader(commentImage.InputStream))
                {
                    comment.CommentImage = binaryReader.ReadBytes(commentImage.ContentLength);
                    comment.CommentImageContentType = commentImage.ContentType;
                    comment.CommentImageFileName = commentImage.FileName;
                }
            }
        }

        [HttpGet]
        public ActionResult GetCommentImage(int id)
        {
            try
            {
                var comment = db.Comments.Find(id);
                if (comment?.CommentImage != null && comment.CommentImage.Length > 0)
                {
                    return File(comment.CommentImage, comment.CommentImageContentType ?? "image/jpeg");
                }

                // Return 404 if no image found
                return new HttpStatusCodeResult(404, "Image not found");
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500, "Error retrieving image");
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";

            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddComment(int novelId, string content, int? parentCommentId = null, HttpPostedFileBase commentImage = null)
        {
            Debug.WriteLine($"[DEBUG] AddComment called for novel ID: {novelId}, parentCommentId: {parentCommentId}");

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment cannot be empty" });
            }

            if (content.Length > 2000)
            {
                return Json(new { success = false, message = "Comment is too long (max 2000 characters)" });
            }

            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Please log in to add comments" });
            }

            try
            {
                if (parentCommentId.HasValue)
                {
                    var parentComment = db.Comments.FirstOrDefault(c => c.Id == parentCommentId.Value && c.NovelId == novelId);
                    if (parentComment == null)
                    {
                        return Json(new { success = false, message = "Parent comment not found" });
                    }
                }

                var comment = new Comment
                {
                    UserId = userId.Value,
                    NovelId = novelId,
                    ParentCommentId = parentCommentId,
                    Content = content,
                    IsApproved = true,
                    ModerationStatus = "Approved",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    LikeCount = 0,
                    DislikeCount = 0
                };

                SaveCommentImageToDatabase(comment, commentImage);

                db.Comments.Add(comment);
                db.SaveChanges();

                var savedComment = db.Comments
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.Id == comment.Id);

                int replyDepth = 0;
                if (parentCommentId.HasValue)
                {
                    replyDepth = CalculateReplyDepth(parentCommentId.Value);
                }

                var commentViewModel = new CommentViewModel
                {
                    Id = savedComment.Id,
                    CommenterName = savedComment.User?.Username ?? "Unknown",
                    CommenterInitials = GetUserInitials(savedComment.User?.Username ?? "Unknown"),
                    Content = savedComment.Content,
                    CreatedDate = savedComment.CreatedAt,
                    LikeCount = savedComment.LikeCount,
                    DislikeCount = savedComment.DislikeCount,
                    ParentCommentId = savedComment.ParentCommentId,
                    HasImage = savedComment.HasImage,
                    ImageContentType = savedComment.CommentImageContentType,
                    ImageFileName = savedComment.CommentImageFileName,
                    CanEdit = true,
                    CanDelete = true,
                    ReplyDepth = replyDepth,
                    UserVoteType = GetUserVoteType(savedComment.Id, userId),
                    Replies = new List<CommentViewModel>()
                };

                return Json(new
                {
                    success = true,
                    message = parentCommentId.HasValue ? "Reply added successfully" : "Comment added successfully",
                    comment = commentViewModel
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error adding comment: {ex.Message}");
                return Json(new { success = false, message = "Failed to add comment: " + ex.Message });
            }
        }
        private int CalculateReplyDepth(int commentId, int currentDepth = 0)
        {
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            if (comment?.ParentCommentId == null)
            {
                return currentDepth;
            }

            return CalculateReplyDepth(comment.ParentCommentId.Value, currentDepth + 1);
        }

        [HttpGet]
        public JsonResult GetCommentThread(int commentId)
        {
            try
            {
                var comment = db.Comments
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.Id == commentId);

                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found" }, JsonRequestBehavior.AllowGet);
                }

                // Get the full thread path
                var threadPath = GetCommentThreadPath(commentId);

                return Json(new
                {
                    success = true,
                    comment = new
                    {
                        Id = comment.Id,
                        Content = comment.Content,
                        CommenterName = comment.User?.Username ?? "Anonymous",
                        CreatedDate = comment.CreatedAt,
                        ParentCommentId = comment.ParentCommentId
                    },
                    threadPath = threadPath
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error getting comment thread: {ex.Message}");
                return Json(new { success = false, message = "Failed to load comment thread" }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<int> GetCommentThreadPath(int commentId)
        {
            var path = new List<int>();
            var currentComment = db.Comments.FirstOrDefault(c => c.Id == commentId);

            while (currentComment != null)
            {
                path.Insert(0, currentComment.Id);

                if (currentComment.ParentCommentId == null)
                    break;

                currentComment = db.Comments.FirstOrDefault(c => c.Id == currentComment.ParentCommentId.Value);
            }

            return path;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditComment(int commentId, string content, HttpPostedFileBase commentImage = null, bool removeImage = false)
        {
            Debug.WriteLine($"[DEBUG] EditComment called for comment ID: {commentId}");

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment cannot be empty" });
            }

            if (content.Length > 2000)
            {
                return Json(new { success = false, message = "Comment is too long (max 2000 characters)" });
            }

            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Please log in to edit comments" });
            }

            try
            {
                var comment = db.Comments.FirstOrDefault(c => c.Id == commentId && c.UserId == userId.Value);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found or you don't have permission to edit" });
                }

                if (comment.UserId != userId.Value)
                {
                    return Json(new { success = false, message = "You can only edit your own comments" });
                }

                comment.Content = content;
                comment.UpdatedAt = DateTime.Now;

                if (removeImage)
                {
                    comment.CommentImage = null;
                    comment.CommentImageContentType = null;
                    comment.CommentImageFileName = null;
                }

                if (commentImage != null && commentImage.ContentLength > 0)
                {
                    SaveCommentImageToDatabase(comment, commentImage);
                }

                db.SaveChanges();

                Debug.WriteLine($"[DEBUG] Successfully updated comment {commentId} with content: {content.Substring(0, Math.Min(50, content.Length))}...");

                return Json(new
                {
                    success = true,
                    message = "Comment updated successfully",
                    content = content,
                    hasImage = comment.CommentImage != null && comment.CommentImage.Length > 0,
                    imageContentType = comment.CommentImageContentType,
                    imageFileName = comment.CommentImageFileName
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error editing comment: {ex.Message}");
                return Json(new { success = false, message = "Failed to edit comment" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteComment(int commentId)
        {
            Debug.WriteLine($"[DEBUG] DeleteComment called for comment ID: {commentId}");

            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Please log in to delete comments" });
            }

            try
            {
                // ENHANCED: Find comment regardless of whether it's a parent or nested comment
                var comment = db.Comments.FirstOrDefault(c => c.Id == commentId && c.UserId == userId.Value);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found or you don't have permission to delete" });
                }

                // Additional security check - ensure comment belongs to the current user
                if (comment.UserId != userId.Value)
                {
                    return Json(new { success = false, message = "You can only delete your own comments" });
                }

                // ENHANCED: Check if this comment has replies
                var hasReplies = db.Comments.Any(c => c.ParentCommentId == commentId && c.ModerationStatus != "Deleted");

                if (hasReplies)
                {
                    // Soft delete - preserve the thread structure but mark as deleted
                    comment.Content = "[Comment deleted by user]";
                    comment.UpdatedAt = DateTime.Now;
                    comment.ModerationStatus = "Deleted";
                }
                else
                {
                    // Hard delete if no replies exist
                    db.Comments.Remove(comment);
                }

                db.SaveChanges();

                Debug.WriteLine($"[DEBUG] Successfully deleted comment {commentId} (soft delete: {hasReplies})");

                return Json(new
                {
                    success = true,
                    message = "Comment deleted successfully",
                    isHardDelete = !hasReplies
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error deleting comment: {ex.Message}");
                return Json(new { success = false, message = "Failed to delete comment" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LikeComment(int commentId, bool isLike)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Please log in to like/dislike comments" });
            }

            try
            {
                var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Comment not found" });
                }

                if (comment.ModerationStatus == "Deleted")
                {
                    return Json(new { success = false, message = "Cannot vote on deleted comment" });
                }

                comment.LikedUserIds = comment.LikedUserIds ?? "";
                comment.DislikedUserIds = comment.DislikedUserIds ?? "";

                var likedUsers = comment.LikedUserIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => int.Parse(x)).ToList();
                var dislikedUsers = comment.DislikedUserIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(x => int.Parse(x)).ToList();

                bool hadLiked = likedUsers.Contains(userId.Value);
                bool hadDisliked = dislikedUsers.Contains(userId.Value);

                Debug.WriteLine($"[DEBUG] LikeComment - CommentId: {commentId}, UserId: {userId.Value}, IsLike: {isLike}");
                Debug.WriteLine($"[DEBUG] Database state - HadLiked: {hadLiked}, HadDisliked: {hadDisliked}");
                Debug.WriteLine($"[DEBUG] Current counts - Likes: {comment.LikeCount}, Dislikes: {comment.DislikeCount}");

                string newVoteState = null;

                if (isLike)
                {
                    if (hadLiked)
                    {
                        likedUsers.Remove(userId.Value);
                        comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                        newVoteState = null;
                        Debug.WriteLine($"[DEBUG] TOGGLE - User {userId.Value} unliking comment {commentId}");
                    }
                    else
                    {
                        likedUsers.Add(userId.Value);
                        comment.LikeCount++;
                        newVoteState = "like";

                        if (hadDisliked)
                        {
                            dislikedUsers.Remove(userId.Value);
                            comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
                            Debug.WriteLine($"[DEBUG] SWITCH - User {userId.Value} switching from dislike to like");
                        }
                        else
                        {
                            Debug.WriteLine($"[DEBUG] NEW VOTE - User {userId.Value} liking comment {commentId}");
                        }
                    }
                }
                else
                {
                    if (hadDisliked)
                    {
                        dislikedUsers.Remove(userId.Value);
                        comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
                        newVoteState = null;
                        Debug.WriteLine($"[DEBUG] TOGGLE - User {userId.Value} removing dislike from comment {commentId}");
                    }
                    else
                    {
                        dislikedUsers.Add(userId.Value);
                        comment.DislikeCount++;
                        newVoteState = "dislike";

                        if (hadLiked)
                        {
                            likedUsers.Remove(userId.Value);
                            comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                            Debug.WriteLine($"[DEBUG] SWITCH - User {userId.Value} switching from like to dislike");
                        }
                        else
                        {
                            Debug.WriteLine($"[DEBUG] NEW VOTE - User {userId.Value} disliking comment {commentId}");
                        }
                    }
                }

                comment.LikedUserIds = string.Join(",", likedUsers);
                comment.DislikedUserIds = string.Join(",", dislikedUsers);
                comment.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                Debug.WriteLine($"[DEBUG] Final state - NewVoteState: {newVoteState}");
                Debug.WriteLine($"[DEBUG] Final counts - Likes: {comment.LikeCount}, Dislikes: {comment.DislikeCount}");

                return Json(new
                {
                    success = true,
                    likeCount = comment.LikeCount,
                    dislikeCount = comment.DislikeCount,
                    userVote = newVoteState
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error processing vote: {ex.Message}");
                return Json(new { success = false, message = "Failed to process vote: " + ex.Message });
            }
        }

        private string GetUserVoteType(int commentId, int? userId = null)
        {
            if (userId == null)
                userId = GetCurrentUserId();

            if (userId == null)
                return null;

            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null) return null;

            var likedUsers = (comment.LikedUserIds ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Select(x => int.TryParse(x, out int id) ? id : 0).ToList();
            var dislikedUsers = (comment.DislikedUserIds ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(x => int.TryParse(x, out int id) ? id : 0).ToList();

            if (likedUsers.Contains(userId.Value)) return "like";
            if (dislikedUsers.Contains(userId.Value)) return "dislike";
            return null;
        }

        [HttpGet]
        public JsonResult LoadMoreComments(int novelId, int skip = 0, int take = 10, string type = "comments")
        {
            Debug.WriteLine($"[DEBUG] AJAX LoadMoreComments called for novel ID: {novelId}, skip: {skip}, take: {take}, type: {type}");

            try
            {
                if (type.ToLower() == "reviews")
                {
                    var reviews = GetReviews(novelId, take, skip);
                    return Json(new
                    {
                        success = true,
                        reviews = reviews,
                        hasMore = reviews.Count == take
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var comments = GetComments(novelId, take, skip);
                    var totalComments = GetTotalCommentsCount(novelId);
                    var hasMore = (skip + take) < totalComments;

                    return Json(new
                    {
                        success = true,
                        comments = comments,
                        hasMore = hasMore,
                        totalComments = totalComments,
                        currentSkip = skip,
                        currentTake = take
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error loading more comments: {ex.Message}");
                return Json(new { success = false, message = "Failed to load comments" }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<CommentViewModel> GetAllNestedReplies(int parentCommentId, int? currentUserId = null, int depth = 0, int maxDepth = 9999)
        {
            if (depth >= maxDepth || depth > 50)
            {
                return new List<CommentViewModel>();
            }

            if (currentUserId == null)
            {
                currentUserId = GetCurrentUserId();
            }

            var directReplies = db.Comments
                .Where(c => c.ParentCommentId == parentCommentId && c.IsApproved && c.ModerationStatus != "Deleted")
                .OrderBy(c => c.CreatedAt)
                .Include(c => c.User)
                .ToList();

            var replyViewModels = new List<CommentViewModel>();

            foreach (var reply in directReplies)
            {
                var replyViewModel = new CommentViewModel
                {
                    Id = reply.Id,
                    UserId = reply.UserId,
                    CommenterName = reply.User?.Username ?? "Anonymous",
                    CommenterInitials = GetUserInitials(reply.User?.Username ?? "Anonymous"),
                    Content = reply.Content,
                    CreatedDate = reply.CreatedAt,
                    LikeCount = reply.LikeCount,
                    DislikeCount = reply.DislikeCount,
                    ParentCommentId = reply.ParentCommentId,
                    HasImage = reply.HasImage,
                    ImageContentType = reply.CommentImageContentType,
                    ImageFileName = reply.CommentImageFileName,
                    CanEdit = currentUserId.HasValue && currentUserId.Value == reply.UserId,
                    CanDelete = currentUserId.HasValue && currentUserId.Value == reply.UserId,
                    HasUserVoted = HasUserVotedOnComment(reply.Id, currentUserId),
                    UserVoteType = GetUserVoteType(reply.Id, currentUserId),
                    LikedUserIds = reply.LikedUserIds,
                    DislikedUserIds = reply.DislikedUserIds,
                    ReplyDepth = depth + 1,
                    Replies = GetAllNestedReplies(reply.Id, currentUserId, depth + 1, maxDepth)
                };

                replyViewModels.Add(replyViewModel);
            }

            return replyViewModels;
        }

        private List<CommentViewModel> GetComments(int novelId, int take = 10, int skip = 0)
        {
            int? currentUserId = GetCurrentUserId();
            var topLevelComments = db.Comments
                .Where(c => c.NovelId == novelId && c.IsApproved && c.ParentCommentId == null && c.ModerationStatus != "Deleted")
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(c => c.User)
                .ToList();
            var result = topLevelComments.Select(c =>
            {
                var viewModel = new CommentViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    CommenterName = c.User?.Username ?? "Anonymous",
                    CommenterInitials = GetUserInitials(c.User?.Username ?? "Anonymous"),
                    Content = c.Content,
                    CreatedDate = c.CreatedAt,
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    ParentCommentId = c.ParentCommentId,
                    HasImage = c.HasImage,
                    ImageContentType = c.CommentImageContentType,
                    ImageFileName = c.CommentImageFileName,
                    CanEdit = currentUserId.HasValue && currentUserId.Value == c.UserId,
                    CanDelete = currentUserId.HasValue && currentUserId.Value == c.UserId,
                    HasUserVoted = HasUserVotedOnComment(c.Id, currentUserId),
                    UserVoteType = GetUserVoteType(c.Id, currentUserId),
                    LikedUserIds = c.LikedUserIds,
                    DislikedUserIds = c.DislikedUserIds,
                    ReplyDepth = 0,
                    Replies = GetAllNestedReplies(c.Id, currentUserId, 0, 9999)
                };
                return viewModel;
            }).ToList();
            return result;
        }

        private int GetTotalCommentsCount(int novelId)
        {
            return db.Comments.Count(c => c.NovelId == novelId && c.IsApproved && c.ParentCommentId == null && c.ModerationStatus != "Deleted");
        }

        [HttpGet]
        public JsonResult DebugNestedComments(int novelId)
        {
            try
            {
                int? currentUserId = GetCurrentUserId();
                Debug.WriteLine($"[DEBUG] DebugNestedComments - Current user ID: {currentUserId}");

                var allComments = db.Comments
                    .Where(c => c.NovelId == novelId && c.IsApproved && c.ModerationStatus != "Deleted")
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                var debugInfo = allComments.Select(c => new {
                    Id = c.Id,
                    Content = c.Content.Substring(0, Math.Min(50, c.Content.Length)) + "...",
                    UserId = c.UserId,
                    Username = c.User?.Username,
                    ParentCommentId = c.ParentCommentId,
                    IsTopLevel = c.ParentCommentId == null,
                    CanUserEdit = currentUserId.HasValue && currentUserId.Value == c.UserId,
                    CanUserDelete = currentUserId.HasValue && currentUserId.Value == c.UserId
                }).ToList();

                return Json(new
                {
                    success = true,
                    currentUserId = currentUserId,
                    totalComments = allComments.Count,
                    comments = debugInfo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] DebugNestedComments error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private int GetRootParentCommentId(int commentId)
        {
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null || comment.ParentCommentId == null)
            {
                return commentId;
            }

            // Recursively find the root parent
            return GetRootParentCommentId(comment.ParentCommentId.Value);
        }

        private bool HasUserVotedOnComment(int commentId, int? userId = null)
        {
            if (userId == null)
                userId = GetCurrentUserId();

            if (userId == null)
                return false;

            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null) return false;

            var likedUsers = (comment.LikedUserIds ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Select(x => int.TryParse(x, out int id) ? id : 0).ToList();
            var dislikedUsers = (comment.DislikedUserIds ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(x => int.TryParse(x, out int id) ? id : 0).ToList();

            return likedUsers.Contains(userId.Value) || dislikedUsers.Contains(userId.Value);
        }

        private List<ReviewViewModel> GetReviews(int novelId, int take = 10, int skip = 0)
        {
            Debug.WriteLine($"[DEBUG] Getting reviews for novel ID: {novelId}, take: {take}, skip: {skip}");

            var commentsWithRatings = db.Comments
                .Where(c => c.NovelId == novelId && c.IsApproved && c.ParentCommentId == null && c.ModerationStatus != "Deleted")
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList()
                .Select(c => new
                {
                    Comment = c,
                    Rating = db.Ratings.FirstOrDefault(r => r.ReaderId == c.UserId && r.NovelId == c.NovelId)
                })
                .Where(cr => cr.Rating != null)
                .Select(cr => new ReviewViewModel
                {
                    Id = cr.Comment.Id,
                    ReviewerName = cr.Comment.User?.Username ?? "Anonymous",
                    ReviewerInitials = GetUserInitials(cr.Comment.User?.Username ?? "Anonymous"),
                    Title = ExtractReviewTitle(cr.Comment.Content),
                    Content = cr.Comment.Content,
                    Rating = cr.Rating.RatingValue,
                    CreatedDate = cr.Comment.CreatedAt,
                    LikeCount = cr.Comment.LikeCount,
                    DislikeCount = cr.Comment.DislikeCount
                })
                .ToList();

            return commentsWithRatings;
        }

        [HttpGet]
        public JsonResult DebugComments(int novelId)
        {
            try
            {
                var commentsCount = db.Comments.Count(c => c.NovelId == novelId);
                var approvedComments = db.Comments.Count(c => c.NovelId == novelId && c.IsApproved);
                var topLevelComments = db.Comments.Count(c => c.NovelId == novelId && c.ParentCommentId == null);
                var nestedComments = db.Comments.Count(c => c.NovelId == novelId && c.ParentCommentId != null);

                var sampleComment = db.Comments
                    .Where(c => c.NovelId == novelId)
                    .Select(c => new {
                        c.Id,
                        c.Content,
                        c.UserId,
                        c.IsApproved,
                        c.ModerationStatus,
                        c.ParentCommentId,
                        Username = c.User != null ? c.User.Username : "NULL USER"
                    })
                    .FirstOrDefault();

                return Json(new
                {
                    success = true,
                    totalComments = commentsCount,
                    approvedComments = approvedComments,
                    topLevelComments = topLevelComments,
                    nestedComments = nestedComments,
                    sampleComment = sampleComment,
                    currentUserId = GetCurrentUserId()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



        #endregion
























        private int? GetNovelRank(int novelId)
        {
            var ranking = db.Rankings
                .FirstOrDefault(r => r.NovelId == novelId && r.RankingType == "popular" && r.RankingPeriod == "monthly");

            return ranking?.Rank;
        }

        private int GetAuthorNovelCount(int authorId)
        {
            return db.Novels.Count(n => n.AuthorId == authorId && n.IsActive);
        }

        private long GetAuthorTotalReads(int authorId)
        {
            return db.Novels
                .Where(n => n.AuthorId == authorId && n.IsActive)
                .Sum(n => (long?)n.ViewCount) ?? 0;
        }

        private bool IsNovelBookmarked(int novelId, int? userId)
        {
            if (userId == null) return false;

            return db.Bookmarks.Any(b => b.NovelId == novelId && b.ReaderId == userId.Value);
        }

        private decimal? GetUserRating(int novelId, int? userId)
        {
            if (userId == null) return null;

            var rating = db.Ratings.FirstOrDefault(r => r.NovelId == novelId && r.ReaderId == userId.Value);
            return rating?.RatingValue;
        }

        private int? GetLastReadChapter(int novelId, int? userId)
        {
            if (userId == null) return null;

            var progress = db.ReadingProgress.FirstOrDefault(rp => rp.NovelId == novelId && rp.ReaderId == userId.Value);
            return progress?.LastReadChapterNumber;
        }

        private bool IsChapterUnlocked(int chapterId, int? userId)
        {
            // Implement your chapter unlock logic here
            // This could check if user has purchased the chapter or has premium subscription
            return true; // Placeholder
        }

        private void UpdateViewCount(int novelId)
        {
            try
            {
                // Use raw SQL to avoid Entity Framework tracking issues
                var sql = "UPDATE Novels SET ViewCount = ViewCount + 1, UpdatedAt = @updatedAt WHERE Id = @novelId";
                db.Database.ExecuteSqlCommand(sql,
                    new System.Data.SqlClient.SqlParameter("@updatedAt", DateTime.Now),
                    new System.Data.SqlClient.SqlParameter("@novelId", novelId));

                Debug.WriteLine($"[DEBUG] View count updated successfully for novel ID: {novelId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error updating view count for novel {novelId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");
                }
                // Don't throw the exception - view count update shouldn't break the page load
            }
        }


        private int? GetCurrentUserId()
        {
            try
            {
                // Check if user is logged in via session
                if (Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
                {
                    Debug.WriteLine("[DEBUG] User is not logged in via session");
                    return null;
                }

                // Get user ID from session
                if (Session["UserId"] != null)
                {
                    int userId = (int)Session["UserId"];
                    Debug.WriteLine($"[DEBUG] Found user ID in session: {userId}");

                    // Verify user still exists in database
                    var user = db.Users.FirstOrDefault(u => u.Id == userId && u.IsActive);
                    if (user != null)
                    {
                        Debug.WriteLine($"[DEBUG] Verified user exists: {user.Username}");
                        return userId;
                    }
                    else
                    {
                        Debug.WriteLine($"[DEBUG] User ID {userId} not found in database or inactive");
                        // Clear invalid session
                        Session.Clear();
                        return null;
                    }
                }

                Debug.WriteLine("[DEBUG] No user ID found in session");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error getting current user ID: {ex.Message}");
                return null;
            }
        }

        protected bool IsUserLoggedIn()
        {
            return GetCurrentUserId() != null;
        }

        protected string GetCurrentUsername()
        {
            if (Session["Username"] != null)
            {
                return Session["Username"].ToString();
            }
            return null;
        }

        [HttpGet]
        public JsonResult DebugAuth()
        {
            try
            {
                var debugInfo = new
                {
                    // ASP.NET Identity (what you were checking before)
                    AspNetIsAuthenticated = User.Identity.IsAuthenticated,
                    AspNetUsername = User.Identity.Name,
                    AspNetAuthenticationType = User.Identity.AuthenticationType,

                    // Session-based authentication (what you're actually using)
                    SessionIsLoggedIn = Session["IsLoggedIn"]?.ToString(),
                    SessionUserId = Session["UserId"]?.ToString(),
                    SessionUsername = Session["Username"]?.ToString(),

                    // Session info
                    SessionId = Session.SessionID,
                    SessionKeys = Session.Keys.Cast<string>().ToArray(),
                    SessionTimeout = Session.Timeout,

                    // Method results
                    GetCurrentUserIdResult = GetCurrentUserId(),
                    IsUserLoggedInResult = IsUserLoggedIn(), // If using BaseController

                    // Database info
                    TotalUsersInDb = db.Users.Count(),
                    SampleUsers = db.Users.Take(5).Select(u => new { u.Id, u.Username, u.Email, u.IsActive }).ToList(),

                    // Request info
                    RequestPath = Request.Url.ToString(),
                    RequestMethod = Request.HttpMethod,

                    // Cookie info (for remember me)
                    HasRememberMeCookie = Request.Cookies["RememberMe"] != null,
                    RememberMeCookieValue = Request.Cookies["RememberMe"]?.Value
                };

                Debug.WriteLine($"[DEBUG AUTH] Session IsLoggedIn: {debugInfo.SessionIsLoggedIn}");
                Debug.WriteLine($"[DEBUG AUTH] Session UserId: {debugInfo.SessionUserId}");
                Debug.WriteLine($"[DEBUG AUTH] Session Username: {debugInfo.SessionUsername}");
                Debug.WriteLine($"[DEBUG AUTH] GetCurrentUserId Result: {debugInfo.GetCurrentUserIdResult}");

                return Json(debugInfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] DebugAuth failed: {ex.Message}");
                return Json(new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DebugAuthDetailed()
        {
            try
            {
                var debugInfo = new
                {
                    // Basic authentication info
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    Username = User.Identity.Name,
                    AuthenticationType = User.Identity.AuthenticationType,

                    // Session info
                    SessionId = Session.SessionID,
                    SessionUserId = Session["UserId"]?.ToString(),
                    SessionKeys = Session.Keys.Cast<string>().ToArray(),

                    // Request info
                    Headers = Request.Headers.AllKeys.ToDictionary(k => k, k => Request.Headers[k]),
                    Cookies = Request.Cookies.AllKeys.ToDictionary(k => k, k => Request.Cookies[k]?.Value),

                    // Claims (if using claims-based auth)
                    Claims = User.Identity is System.Security.Claims.ClaimsIdentity claimsIdentity
                        ? claimsIdentity.Claims.Select(c => new { c.Type, c.Value }).ToList()
                        : null,

                    // Database info
                    TotalUsersInDb = db.Users.Count(),
                    SampleUsers = db.Users.Take(5).Select(u => new { u.Id, u.Username, u.Email }).ToList(),

                    // Current user attempt
                    CurrentUserId = GetCurrentUserId(),
                    CurrentUserMethod = "GetCurrentUserId()",

                    // Alternative methods to find user
                    UserBySession = Session["UserId"] != null ? db.Users.Find(Session["UserId"]) : null,

                    // Request path
                    RequestPath = Request.Url.ToString(),
                    RequestMethod = Request.HttpMethod
                };

                Debug.WriteLine($"[DEBUG AUTH DETAILED] IsAuthenticated: {debugInfo.IsAuthenticated}");
                Debug.WriteLine($"[DEBUG AUTH DETAILED] Username: {debugInfo.Username}");
                Debug.WriteLine($"[DEBUG AUTH DETAILED] CurrentUserId: {debugInfo.CurrentUserId}");
                Debug.WriteLine($"[DEBUG AUTH DETAILED] SessionUserId: {debugInfo.SessionUserId}");

                return Json(debugInfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] DebugAuthDetailed failed: {ex.Message}");
                return Json(new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }












        private string GetUserInitials(string username)
        {
            if (string.IsNullOrEmpty(username)) return "??";

            var parts = username.Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();

            return username.Substring(0, Math.Min(2, username.Length)).ToUpper();
        }

        private string ExtractReviewTitle(string content)
        {
            // Extract first line or first sentence as title
            if (string.IsNullOrEmpty(content)) return "Review";

            var firstLine = content.Split('\n')[0];
            if (firstLine.Length > 50)
                firstLine = firstLine.Substring(0, 47) + "...";

            return firstLine;
        }

        // Helper method to get user avatar URL
        private string GetUserAvatarUrl(int userId)
        {
            // You can create an action method similar to GetCoverImage for user avatars
            return Url.Action("GetUserAvatar", "User", new { id = userId });
        }

        // AJAX endpoints for dynamic functionality
        [HttpPost]
        public JsonResult AddToLibrary(int novelId)
        {
            Debug.WriteLine($"[DEBUG] AddToLibrary called for novel ID: {novelId}");
            int? userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Please log in to add to library" });

            var existingBookmark = db.Bookmarks.FirstOrDefault(b => b.NovelId == novelId && b.ReaderId == userId.Value);

            if (existingBookmark == null)
            {
                db.Bookmarks.Add(new Bookmark
                {
                    NovelId = novelId,
                    ReaderId = userId.Value,
                    BookmarkType = "Reading",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });

                // Update bookmark count
                var novel = db.Novels.Find(novelId);
                if (novel != null)
                {
                    novel.BookmarkCount++;
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Added to library", isBookmarked = true });
            }
            else
            {
                db.Bookmarks.Remove(existingBookmark);

                // Update bookmark count
                var novel = db.Novels.Find(novelId);
                if (novel != null)
                {
                    novel.BookmarkCount--;
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Removed from library", isBookmarked = false });
            }
        }

        [HttpPost]
        public JsonResult RateNovel(int novelId, decimal rating)
        {
            Debug.WriteLine($"[DEBUG] RateNovel called for novel ID: {novelId}, rating: {rating}");
            int? userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "Please log in to rate" });

            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = "Rating must be between 1 and 5" });

            var existingRating = db.Ratings.FirstOrDefault(r => r.NovelId == novelId && r.ReaderId == userId.Value);

            if (existingRating == null)
            {
                db.Ratings.Add(new Rating
                {
                    NovelId = novelId,
                    ReaderId = userId.Value,
                    RatingValue = rating,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                existingRating.RatingValue = rating;
                existingRating.UpdatedAt = DateTime.Now;
            }

            // Update novel's average rating and total ratings
            UpdateNovelRating(novelId);

            db.SaveChanges();
            return Json(new { success = true, message = "Rating saved" });
        }

        private void UpdateNovelRating(int novelId)
        {
            var ratings = db.Ratings.Where(r => r.NovelId == novelId).ToList();
            var novel = db.Novels.Find(novelId);

            if (novel != null && ratings.Any())
            {
                novel.AverageRating = ratings.Average(r => r.RatingValue);
                novel.TotalRatings = ratings.Count;
                novel.UpdatedAt = DateTime.Now;
            }
        }

        // Add this method to serve user avatars similar to your GetCoverImage method
        [HttpGet]
        public ActionResult GetUserAvatar(int id)
        {
            Debug.WriteLine($"[DEBUG] GetUserAvatar called for user ID: {id}");

            try
            {
                var user = db.Users.Find(id);
                Debug.WriteLine($"[DEBUG] User found: {user != null}");

                if (user != null)
                {
                    Debug.WriteLine($"[DEBUG] User username: {user.Username}");
                    Debug.WriteLine($"[DEBUG] ProfilePictureData is null: {user.ProfilePictureData == null}");

                    if (user.ProfilePictureData != null)
                    {
                        Debug.WriteLine($"[DEBUG] ProfilePictureData length: {user.ProfilePictureData.Length}");
                    }
                }

                if (user?.ProfilePictureData != null && user.ProfilePictureData.Length > 0)
                {
                    string contentType = user.ProfilePictureContentType ?? "image/jpeg";
                    Debug.WriteLine($"[DEBUG] Returning user avatar with content type: {contentType}");
                    return File(user.ProfilePictureData, contentType);
                }

                // Return default avatar
                Debug.WriteLine($"[DEBUG] No user avatar found, trying default avatar");
                string defaultImagePath = Server.MapPath("~/Content/images/default-avatar.png");

                if (System.IO.File.Exists(defaultImagePath))
                {
                    Debug.WriteLine($"[DEBUG] Default avatar found at: {defaultImagePath}");
                    return File(defaultImagePath, "image/png");
                }

                // Try jpg version
                defaultImagePath = Server.MapPath("~/Content/images/default-avatar.jpg");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    Debug.WriteLine($"[DEBUG] Default avatar (jpg) found at: {defaultImagePath}");
                    return File(defaultImagePath, "image/jpeg");
                }

                Debug.WriteLine($"[DEBUG] Default avatar not found, returning 404");
                return new HttpStatusCodeResult(404);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Exception in GetUserAvatar for ID {id}: {ex.Message}");
                Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return new HttpStatusCodeResult(500);
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

        #region



        #endregion


    }
}