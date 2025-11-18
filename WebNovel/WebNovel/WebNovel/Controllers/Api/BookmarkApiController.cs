using System;
using System.Data.Entity.Infrastructure; // Thêm dòng này để bắt lỗi DbUpdateException
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Controllers
{
    public class BookmarkApiController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // API: /BookmarkApi/Toggle
        [HttpPost]
        public JsonResult Toggle(int novelId)
        {
            try
            {
                // 1. Kiểm tra đăng nhập (Lấy UserId = 13)
                if (Session["IsLoggedIn"] == null || (bool)Session["IsLoggedIn"] == false)
                {
                    return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để thực hiện chức năng này." });
                }

                int userId = (int)Session["UserId"]; // userId là 13

                // ==========================================================
                // [SỬA LỖI] THÊM BƯỚC TRUY VẤN READER ID (TỪ 13 SANG 10)
                // ==========================================================
                var reader = db.Readers.FirstOrDefault(r => r.UserId == userId);
                if (reader == null)
                {
                    return Json(new { success = false, message = "Lỗi: Không tìm thấy thông tin Độc giả (Reader) cho tài khoản này." });
                }
                int currentReaderId = reader.Id; // <-- Đây mới là ReaderId (số 10)
                // ==========================================================


                // 2. Tìm bookmark (SỬA: Dùng currentReaderId thay vì userId)
                var existingBookmark = db.Bookmarks
                    .FirstOrDefault(b => b.NovelId == novelId && b.ReaderId == currentReaderId); // Dùng số 10

                var novel = db.Novels.Find(novelId);
                bool isBookmarked = false;

                if (existingBookmark == null)
                {
                    // --- A. CHƯA CÓ -> THÊM MỚI ---
                    var newBookmark = new Bookmark
                    {
                        NovelId = novelId,
                        ReaderId = currentReaderId, // [SỬA] Dùng số 10
                        BookmarkType = "Reading",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    db.Bookmarks.Add(newBookmark);

                    if (novel != null)
                    {
                        novel.BookmarkCount++;
                    }
                    isBookmarked = true;
                }
                else
                {
                    // --- B. ĐÃ CÓ -> XÓA BỎ ---
                    db.Bookmarks.Remove(existingBookmark);

                    if (novel != null && novel.BookmarkCount > 0)
                    {
                        novel.BookmarkCount--;
                    }
                    isBookmarked = false;
                }

                // 3. Lưu thay đổi
                db.SaveChanges(); // <-- Dòng này sẽ hết lỗi

                // 4. Trả về JSON 
                return Json(new
                {
                    success = true,
                    isBookmarked = isBookmarked,
                    newBookmarkCountFormatted = novel != null ? FormatNumber(novel.BookmarkCount) : "0",
                    message = isBookmarked ? "Đã thêm vào thư viện" : "Đã xóa khỏi thư viện"
                });
            }
            // [SỬA] Thêm đoạn bắt lỗi chi tiết để báo lỗi SQL
            catch (DbUpdateException dbEx)
            {
                // Lấy lỗi gốc từ SQL Server
                var innerMessage = dbEx.InnerException?.InnerException?.Message ?? dbEx.InnerException?.Message ?? dbEx.Message;
                System.Diagnostics.Debug.WriteLine("DB ERROR: " + innerMessage);
                return Json(new { success = false, message = "Lỗi DB: " + innerMessage });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GENERAL ERROR: " + ex.Message);
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Hàm phụ trợ để format số lượng (ví dụ: 1200 -> 1.2K)
        private string FormatNumber(long num)
        {
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";
            return num.ToString("#,0");
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