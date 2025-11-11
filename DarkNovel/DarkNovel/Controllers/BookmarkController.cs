using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DarkNovel.Data;
using DarkNovel.Models;
using DarkNovel.Models.ApiModels; // Namespace chứa DTO

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BookmarksController : ControllerBase
{
    private readonly DarkNovelContext _context;

    public BookmarksController(DarkNovelContext context)
    {
        _context = context;
    }

    // *** PHƯƠNG THỨC HỖ TRỢ (ĐÃ SỬA) ***
    private int GetReaderId()
    {
        // *** SỬA LỖI LOGIC: Đọc claim "ReaderId" mới mà ta đã thêm vào Token ***
        // (Claim này được thêm vào trong UserController.cs)
        var readerIdClaim = User.FindFirst("ReaderId");

        if (readerIdClaim != null && int.TryParse(readerIdClaim.Value, out int readerId))
        {
            return readerId;
        }
        // Nếu không tìm thấy, ném lỗi 401
        throw new UnauthorizedAccessException("Token không hợp lệ hoặc thiếu ReaderId.");
    }

    // -------------------------------------------------------------------
    // --- 1. LẤY DANH SÁCH BOOKMARK (Khớp với getMyBookmarks) ---
    [HttpGet("my-bookmarks")]
    public async Task<IActionResult> GetMyBookmarks()
    {
        try
        {
            int readerId = GetReaderId(); // Đã đọc đúng ReaderId từ Token

            var bookmarks = await _context.Bookmarks
                .Where(b => b.ReaderId == readerId)
                // *** SỬA: Thêm .Include() để lấy thông tin Novel ***
                .Include(b => b.Novel)
                .Select(b => new BookmarkDto // *** SỬA: Dùng BookmarkDto ***
                {
                    // Ánh xạ các trường khớp với Data Class 'Bookmark.kt'
                    BookmarkId = b.Id, // Đổi tên 'Id' thành 'BookmarkId'
                    ReaderId = b.ReaderId,
                    NovelId = b.NovelId,
                    BookmarkType = b.BookmarkType,
                    CreatedAt = b.CreatedAt,

                    // *** SỬA: Thêm đối tượng Novel lồng vào ***
                    Novel = b.Novel == null ? null : new NovelBookmarkDto
                    {
                        Id = b.Novel.Id,
                        Title = b.Novel.Title,
                        // (Thêm các trường khác mà App 'BookmarkAdapter' cần)
                        CoverImageUrl = b.Novel.CoverImageUrl
                    }
                })
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking() // Tăng hiệu suất
                .ToListAsync();

            // Tạo đối tượng phản hồi khớp với 'BookmarkResponse.kt'
            var response = new BookmarkResponse
            {
                Bookmarks = bookmarks,
                TotalBookmarks = bookmarks.Count
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Lỗi server khi lấy danh sách bookmark.");
        }
    }

    // -------------------------------------------------------------------
    // --- 2. TOGGLE BOOKMARK (Khớp với toggleBookmark) ---
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleBookmark([FromBody] BookmarkToggleRequest request)
    {
        try
        {
            int readerId = GetReaderId(); // Đã đọc đúng ReaderId từ Token

            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.ReaderId == readerId &&
                                          b.NovelId == request.NovelId);

            long newBookmarkCount = 0;

            if (existingBookmark != null)
            {
                // XÓA (Remove)
                _context.Bookmarks.Remove(existingBookmark);
                await _context.SaveChangesAsync();

                // Lấy số lượng bookmark mới của truyện
                newBookmarkCount = await _context.Bookmarks.LongCountAsync(b => b.NovelId == request.NovelId);

                return Ok(new
                {
                    success = true,
                    action = "removed",
                    message = "Đã xóa bookmark.",
                    BookmarkCount = newBookmarkCount
                });
            }
            else
            {
                // THÊM (Add)
                var newBookmark = new Bookmark
                {
                    ReaderId = readerId,
                    NovelId = request.NovelId,
                    BookmarkType = request.BookmarkType ?? "Đã lưu", // Dùng "Đã lưu" theo yêu cầu
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookmarks.Add(newBookmark);
                await _context.SaveChangesAsync();

                // Lấy số lượng bookmark mới của truyện
                newBookmarkCount = await _context.Bookmarks.LongCountAsync(b => b.NovelId == request.NovelId);

                return Ok(new
                {
                    success = true,
                    action = "added",
                    message = "Đã thêm bookmark.",
                    BookmarkCount = newBookmarkCount
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi server khi thực hiện toggle bookmark: " + ex.Message);
        }
    }

    // -------------------------------------------------------------------
    // --- 3. KIỂM TRA TRẠNG THÁI BOOKMARK (Khớp với getBookmarkStatus) ---
    [HttpGet("status/{novelId}")]
    public async Task<IActionResult> GetBookmarkStatus(int novelId)
    {
        try
        {
            int readerId = GetReaderId(); // Đã đọc đúng ReaderId từ Token

            var bookmark = await _context.Bookmarks
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ReaderId == readerId &&
                                          b.NovelId == novelId);

            // Tạo DTO khớp với BookmarkStatusResponse bên App
            var status = new BookmarkStatusResponse
            {
                IsBookmarked = bookmark != null,
                BookmarkId = bookmark?.Id, // Đổi tên 'Id' thành 'BookmarkId'
                BookmarkType = bookmark?.BookmarkType
            };

            return Ok(status);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Lỗi server khi kiểm tra trạng thái bookmark.");
        }
    }

    // -------------------------------------------------------------------
    // --- 4. ĐẾM SỐ BOOKMARK (Khớp với getBookmarkCount) ---
    [HttpGet("count")]
    public async Task<IActionResult> GetBookmarkCount()
    {
        try
        {
            int readerId = GetReaderId(); // Đã đọc đúng ReaderId từ Token

            var count = await _context.Bookmarks
                .CountAsync(b => b.ReaderId == readerId);

            return Ok(new { count = count, success = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Lỗi server khi đếm số bookmark.");
        }
    }
}