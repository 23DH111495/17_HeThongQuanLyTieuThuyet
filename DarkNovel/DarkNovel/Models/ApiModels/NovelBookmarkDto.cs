using System;
using System.Text.Json.Serialization;

// Đảm bảo namespace này khớp với các DTO khác của bạn
namespace DarkNovel.Models.ApiModels
{
    // DTO này khớp với data class 'NovelBookmark.kt' bên App
    // Nó chứa các thông tin cần thiết để hiển thị trong danh sách bookmark
    public class NovelBookmarkDto
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        // SỬA LỖI 1: Thêm Tên Tác Giả (AuthorName)
        [JsonPropertyName("AuthorName")]
        public string? AuthorName { get; set; }

        [JsonPropertyName("CoverImageUrl")]
        public string? CoverImageUrl { get; set; }

        // SỬA LỖI 2: Thêm các trường hiển thị trong UI (như hình bạn gửi)
        [JsonPropertyName("ViewCount")]
        public long ViewCount { get; set; }

        [JsonPropertyName("AverageRating")]
        public decimal AverageRating { get; set; } // Dùng decimal cho C#

        // SỬA LỖI 3: Dùng BookmarkCount (Lưu dấu) 
        [JsonPropertyName("BookmarkCount")]
        public long BookmarkCount { get; set; }
    }
}