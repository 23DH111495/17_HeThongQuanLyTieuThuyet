using System.ComponentModel.DataAnnotations;

namespace DarkNovel.Models.ApiModels
{
    public class AddBookmarkRequest
    {
        // ID truyện mà người dùng muốn lưu
        [Required]
        public int NovelId { get; set; }

        // Loại Bookmark, ví dụ: "Favorite", "ReadLater", v.v.
        public string BookmarkType { get; set; } = "Favorite";
    }
}
