//using System;
//using System.Linq;
//using System.Web.Http;
//using WebNovel.Data;
//using WebNovel.Models;
//using WebNovel.Models.ApiModels;
//using System.Data.Entity;

//namespace WebNovel.Controllers.Api
//{
//    [RoutePrefix("api/chapters")]
//    public class ChaptersApiController : ApiController
//    {
//        private DarkNovelDbContext db = new DarkNovelDbContext();

//        // GET: api/chapters/5 - Get chapter content
//        [HttpGet]
//        [Route("{id:int}")]
//        public IHttpActionResult GetChapter(int id, int? userId = null)
//        {
//            try
//            {
//                var chapter = db.Chapters
//                    .Include(c => c.Novel)
//                    .Include(c => c.Novel.Author)
//                    .FirstOrDefault(c => c.Id == id && c.IsPublished);

//                if (chapter == null)
//                {
//                    return NotFound();
//                }

//                // Check if user has access to this chapter
//                bool hasAccess = true;
//                string accessReason = "free";

//                if (chapter.IsPremium || chapter.UnlockPrice > 0)
//                {
//                    if (userId.HasValue)
//                    {
//                        // Check if user has unlocked this chapter
//                        var unlocked = db.UnlockedChapters
//                            .Any(uc => uc.UserId == userId.Value && uc.ChapterId == id);

//                        if (!unlocked)
//                        {
//                            // Check if user has premium subscription
//                            var user = db.Users.Find(userId.Value);
//                            var reader = db.Readers.FirstOrDefault(r => r.UserId == userId.Value);

//                            if (reader?.IsPremium == true && reader.PremiumExpiryDate > DateTime.Now)
//                            {
//                                accessReason = "premium";
//                            }
//                            else
//                            {
//                                hasAccess = false;
//                                accessReason = "locked";
//                            }
//                        }
//                        else
//                        {
//                            accessReason = "purchased";
//                        }
//                    }
//                    else
//                    {
//                        hasAccess = false;
//                        accessReason = "login_required";
//                    }
//                }

//                var response = new ChapterDetailDto
//                {
//                    Id = chapter.Id,
//                    NovelId = chapter.NovelId,
//                    ChapterNumber = chapter.ChapterNumber,
//                    Title = chapter.Title,
//                    Content = hasAccess ? chapter.Content : GetPreviewContent(chapter),
//                    WordCount = chapter.WordCount,
//                    PublishDate = chapter.PublishDate,
//                    IsPremium = chapter.IsPremium,
//                    UnlockPrice = chapter.UnlockPrice,
//                    PreviewContent = hasAccess ? null : GetPreviewContent(chapter)
//                };

//                // Increment view count if user has access
//                if (hasAccess)
//                {
//                    chapter.ViewCount++;
//                    db.SaveChanges();
//                }

//                return Ok(new
//                {
//                    Success = true,
//                    Data = response,
//                    AccessInfo = new
//                    {
//                        HasAccess = hasAccess,
//                        AccessReason = accessReason,
//                        RequiredCoins = chapter.UnlockPrice,
//                        IsPremium = chapter.IsPremium
//                    },
//                    Novel = new
//                    {
//                        Id = chapter.Novel.Id,
//                        Title = chapter.Novel.Title,
//                        AuthorName = chapter.Novel.Author.PenName
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                return InternalServerError(new Exception("Error retrieving chapter: " + ex.Message));
//            }
//        }

//        // GET: api/chapters/5/next - Get next chapter info
//        [HttpGet]
//        [Route("{id:int}/next")]
//        public IHttpActionResult GetNextChapter(int id)
//        {
//            try
//            {
//                var currentChapter = db.Chapters.Find(id);
//                if (currentChapter == null)
//                {
//                    return NotFound();
//                }

//                var nextChapter = db.Chapters
//                    .Where(c => c.NovelId == currentChapter.NovelId &&
//                               c.ChapterNumber > currentChapter.ChapterNumber &&
//                               c.IsPublished)
//                    .OrderBy(c => c.ChapterNumber)
//                    .FirstOrDefault();

//                if (nextChapter == null)
//                {
//                    return Ok(new ApiResponse<object>
//                    {
//                        Success = true,
//                        Data = null,
//                        Message = "No next chapter available"
//                    });
//                }

//                return Ok(new ApiResponse<ChapterSummaryDto>
//                {
//                    Success = true,
//                    Data = new ChapterSummaryDto
//                    {
//                        Id = nextChapter.Id,
//                        ChapterNumber = nextChapter.ChapterNumber,
//                        Title = nextChapter.Title,
//                        WordCount = nextChapter.WordCount,
//                        PublishDate = nextChapter.PublishDate,
//                        IsPremium = nextChapter.IsPremium,
//                        UnlockPrice = nextChapter.UnlockPrice
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                return InternalServerError(new Exception("Error retrieving next chapter: " + ex.Message));
//            }
//        }

//        // GET: api/chapters/5/previous - Get previous chapter info
//        [HttpGet]
//        [Route("{id:int}/previous")]
//        public IHttpActionResult GetPreviousChapter(int id)
//        {
//            try
//            {
//                var currentChapter = db.Chapters.Find(id);
//                if (currentChapter == null)
//                {
//                    return NotFound();
//                }

//                var previousChapter = db.Chapters
//                    .Where(c => c.NovelId == currentChapter.NovelId &&
//                               c.ChapterNumber < currentChapter.ChapterNumber &&
//                               c.IsPublished)
//                    .OrderByDescending(c => c.ChapterNumber)
//                    .FirstOrDefault();

//                if (previousChapter == null)
//                {
//                    return Ok(new ApiResponse<object>
//                    {
//                        Success = true,
//                        Data = null,
//                        Message = "No previous chapter available"
//                    });
//                }

//                return Ok(new ApiResponse<ChapterSummaryDto>
//                {
//                    Success = true,
//                    Data = new ChapterSummaryDto
//                    {
//                        Id = previousChapter.Id,
//                        ChapterNumber = previousChapter.ChapterNumber,
//                        Title = previousChapter.Title,
//                        WordCount = previousChapter.WordCount,
//                        PublishDate = previousChapter.PublishDate,
//                        IsPremium = previousChapter.IsPremium,
//                        UnlockPrice = previousChapter.UnlockPrice
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                return InternalServerError(new Exception("Error retrieving previous chapter: " + ex.Message));
//            }
//        }

//        // POST: api/chapters/5/unlock - Unlock chapter with coins
//        [HttpPost]
//        [Route("{id:int}/unlock")]
//        public IHttpActionResult UnlockChapter(int id, [FromBody] UnlockChapterRequest request)
//        {
//            if (request?.UserId == null)
//            {
//                return BadRequest("User ID is required");
//            }

//            try
//            {
//                var chapter = db.Chapters.Find(id);
//                if (chapter == null)
//                {
//                    return NotFound();
//                }

//                if (chapter.UnlockPrice == 0)
//                {
//                    return BadRequest("This chapter is already free");
//                }

//                // Check if already unlocked
//                var existing = db.UnlockedChapters
//                    .FirstOrDefault(uc => uc.UserId == request.UserId && uc.ChapterId == id);

//                if (existing != null)
//                {
//                    return BadRequest("Chapter already unlocked");
//                }

//                // Check user's coin balance
//                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == request.UserId);
//                if (wallet == null || wallet.CoinBalance < chapter.UnlockPrice)
//                {
//                    return BadRequest("Insufficient coins");
//                }

//                // Convert chapter.UnlockPrice to decimal if it's an int
//                decimal unlockPrice = Convert.ToDecimal(chapter.UnlockPrice);

//                // Deduct coins and unlock chapter
//                wallet.CoinBalance -= unlockPrice;
//                wallet.TotalCoinsSpent += unlockPrice;
//                wallet.LastUpdated = DateTime.Now;
//                wallet.UpdatedAt = DateTime.Now;

//                // Record transaction
//                var transaction = new CoinTransaction
//                {
//                    UserId = request.UserId,
//                    TransactionType = "Spend",
//                    Amount = -Convert.ToInt32(unlockPrice), // Convert to int if CoinTransaction.Amount is int
//                    BalanceBefore = Convert.ToInt32(wallet.CoinBalance + unlockPrice),
//                    BalanceAfter = Convert.ToInt32(wallet.CoinBalance),
//                    RelatedChapterId = id,
//                    Description = $"Unlocked Chapter {chapter.ChapterNumber}: {chapter.Title}"
//                };
//                db.CoinTransactions.Add(transaction);

//                // Unlock chapter
//                var unlock = new UnlockedChapter
//                {
//                    UserId = request.UserId,
//                    ChapterId = id,
//                    UnlockMethod = "Coins",
//                    CoinsSpent = Convert.ToInt32(unlockPrice) // Convert to int if CoinsSpent is int
//                };
//                db.UnlockedChapters.Add(unlock);

//                db.SaveChanges();

//                return Ok(new ApiResponse<object>
//                {
//                    Success = true,
//                    Message = "Chapter unlocked successfully",
//                    Data = new
//                    {
//                        NewCoinBalance = wallet.CoinBalance,
//                        CoinsSpent = unlockPrice
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                return InternalServerError(new Exception("Error unlocking chapter: " + ex.Message));
//            }
//        }

//        private string GetPreviewContent(Chapter chapter)
//        {
//            if (string.IsNullOrEmpty(chapter.Content))
//                return "";

//            // Show first 200 characters as preview
//            var previewLength = Math.Min(200, chapter.Content.Length);
//            return chapter.Content.Substring(0, previewLength) + "...";
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                db.Dispose();
//            }
//            base.Dispose(disposing);
//        }
//    }

//    // Request model for unlocking chapters
//    public class UnlockChapterRequest
//    {
//        public int UserId { get; set; }
//    }
//}