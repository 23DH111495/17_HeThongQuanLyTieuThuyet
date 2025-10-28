using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data; // <<< Namespace DbContext
using WebNovel.Models; // <<< Namespace Models
using System.Web;
using System.Diagnostics; // Cho Debug.WriteLine

namespace WebNovel.Controllers.Api
{
    public class BookmarkApiController : Controller
    {
        private readonly DarkNovelDbContext db;

        // Constructor khởi tạo DbContext
        public BookmarkApiController()
        {
            db = new DarkNovelDbContext();
            // Optional: Disable lazy loading for API calls if not needed
            // db.Configuration.LazyLoadingEnabled = false;
        }
        // (Hoặc dùng constructor inject nếu bạn đã setup DI)
        // public BookmarkApiController(DarkNovelDbContext context) { db = context; }

        // Class nhận BookId từ JS
        public class BookmarkRequest
        {
            public int BookId { get; set; }
        }

        // --- Action TOGGLE bookmark ---
        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo @Html.AntiForgeryToken() có trong View
        public JsonResult Toggle(BookmarkRequest request)
        {
            Debug.WriteLine($"BookmarkApi/Toggle called for BookId: {request?.BookId}"); // Log
            int? userIdInt = GetCurrentUserIdFromSession();

            if (userIdInt == null)
            {
                Debug.WriteLine("Toggle failed: User not logged in (Session missing UserId)."); // Log
                Response.StatusCode = 401; // Unauthorized
                return Json(new { success = false, message = "Bạn cần đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            // Thêm kiểm tra request hợp lệ
            if (request == null || request.BookId <= 0)
            {
                Debug.WriteLine($"Toggle failed: Invalid request data (BookId: {request?.BookId})."); // Log
                Response.StatusCode = 400; // Bad Request
                return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ." }, JsonRequestBehavior.AllowGet);
            }


            using (var transaction = db.Database.BeginTransaction()) // <<< Dùng Transaction
            {
                try
                {
                    // Tìm xem bookmark đã tồn tại chưa (khóa dòng để tránh race condition)
                    var existingBookmark = db.Bookmarks
                                             .FirstOrDefault(b => b.NovelId == request.BookId && b.ReaderId == userIdInt.Value);
                    // .With(SqlLockMode.UpdLock) // Cần using System.Data.Entity.Infrastructure; for locking if needed

                    bool currentlyBookmarked = existingBookmark != null;
                    bool finalStateIsBookmarked;
                    string message;

                    // Lấy thông tin Novel (khóa dòng nếu cần cập nhật count)
                    var novel = db.Novels.Find(request.BookId);
                    //.With(SqlLockMode.UpdLock) // Lock if updating count

                    if (novel == null)
                    {
                        Debug.WriteLine($"Toggle failed: Novel not found (BookId: {request.BookId})."); // Log
                        Response.StatusCode = 404; // Not Found
                        transaction.Rollback(); // Hoàn tác transaction
                        return Json(new { success = false, message = "Không tìm thấy truyện." }, JsonRequestBehavior.AllowGet);
                    }

                    long currentCount = db.Bookmarks.LongCount(b => b.NovelId == request.BookId); // Đếm chính xác trước khi thay đổi

                    if (currentlyBookmarked)
                    {
                        // --- Đã tồn tại -> XÓA ---
                        Debug.WriteLine($"Removing bookmark for User: {userIdInt.Value}, Novel: {request.BookId}"); // Log
                        db.Bookmarks.Remove(existingBookmark);
                        novel.BookmarkCount = Math.Max(0, currentCount - 1); // Cập nhật count
                        novel.UpdatedAt = DateTime.Now;
                        message = "Đã xóa khỏi Bookmark.";
                        finalStateIsBookmarked = false;
                    }
                    else
                    {
                        // --- Chưa tồn tại -> THÊM ---
                        Debug.WriteLine($"Adding bookmark for User: {userIdInt.Value}, Novel: {request.BookId}"); // Log
                        var newBookmark = new Bookmark
                        {
                            NovelId = request.BookId,
                            ReaderId = userIdInt.Value,
                            // CreatedAt, UpdatedAt, BookmarkType tự có giá trị mặc định từ model
                        };
                        db.Bookmarks.Add(newBookmark);
                        novel.BookmarkCount = currentCount + 1; // Cập nhật count
                        novel.UpdatedAt = DateTime.Now;
                        message = "Đã thêm vào Bookmark.";
                        finalStateIsBookmarked = true;
                    }

                    db.SaveChanges(); // Lưu cả bookmark và novel count
                    transaction.Commit(); // Hoàn tất transaction thành công

                    Debug.WriteLine($"Toggle successful. Final state: {finalStateIsBookmarked}, Message: {message}, NewCount: {novel.BookmarkCount}"); // Log

                    // Trả về kết quả
                    return Json(new
                    {
                        success = true,
                        message = message,
                        isBookmarked = finalStateIsBookmarked,
                        newBookmarkCountFormatted = FormatCount(novel.BookmarkCount) // Dùng count cuối cùng
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR in Toggle bookmark: {ex.ToString()}"); // Log lỗi chi tiết
                    transaction.Rollback(); // Hoàn tác transaction nếu có lỗi
                    Response.StatusCode = 500; // Internal Server Error
                    return Json(new { success = false, message = "Lỗi hệ thống khi xử lý bookmark." }, JsonRequestBehavior.AllowGet);
                }
            } // Kết thúc using transaction
        }

        // --- Các hàm hỗ trợ (Giữ nguyên) ---
        private int? GetCurrentUserIdFromSession()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int parsedId)) { return parsedId; }
            Debug.WriteLine("GetCurrentUserIdFromSession: Warning - UserId not found in Session."); // Log
            return null;
        }

        private string FormatCount(long count)
        {
            if (count < 0) count = 0;
            if (count >= 1000000) return (count / 1000000D).ToString("0.##M");
            if (count >= 1000) return (count / 1000D).ToString("0.#K");
            return count.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}