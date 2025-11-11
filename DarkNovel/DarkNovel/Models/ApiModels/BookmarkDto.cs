using System;
using System.Text.Json.Serialization;

namespace DarkNovel.Models.ApiModels
{
    // DTO này khớp với data class 'Bookmark.kt' bên App
    public class BookmarkDto
    {
        [JsonPropertyName("BookmarkId")]
        public int BookmarkId { get; set; }

        [JsonPropertyName("ReaderId")]
        public int ReaderId { get; set; }

        [JsonPropertyName("NovelId")]
        public int NovelId { get; set; }

        [JsonPropertyName("BookmarkType")]
        public string BookmarkType { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        // Gửi kèm chi tiết Novel
        [JsonPropertyName("Novel")]
        public NovelBookmarkDto Novel { get; set; }
    }
}