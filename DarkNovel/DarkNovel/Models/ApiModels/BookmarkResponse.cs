using System.Collections.Generic;
using System.Text.Json.Serialization;

// Đảm bảo namespace này khớp với các DTO khác của bạn
namespace DarkNovel.Models.ApiModels
{
    // DTO này khớp với data class 'BookmarkResponse.kt'
    public class BookmarkResponse
    {
        [JsonPropertyName("TotalBookmarks")]
        public int TotalBookmarks { get; set; }

        // *** SỬA LỖI NẰM Ở ĐÂY ***
        // Kiểu dữ liệu phải là List<BookmarkDto>
        // để khớp với dữ liệu được tạo ra từ BookmarksController.cs
        [JsonPropertyName("Bookmarks")]
        public List<BookmarkDto> Bookmarks { get; set; }
    }
}