namespace DarkNovel.Models.ApiModels
{
    // Phải khớp với Data Class 'BookmarkToggleRequest' bên Kotlin
    public class BookmarkToggleRequest
    {
        // Phải khớp với @SerializedName("NovelId")
        public int NovelId { get; set; }

        // Phải khớp với @SerializedName("BookmarkType")
        public string BookmarkType { get; set; }
    }
}