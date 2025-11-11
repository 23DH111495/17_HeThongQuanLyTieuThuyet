namespace DarkNovel.Models.ApiModels
{
    // Phải khớp với Data Class 'BookmarkStatusResponse' bên Kotlin
    public class BookmarkStatusResponse
    {
        // Phải khớp với @SerializedName("IsBookmarked")
        public bool IsBookmarked { get; set; }

        // Phải khớp với @SerializedName("BookmarkId")
        public int? BookmarkId { get; set; } // Dùng int? vì có thể null

        // Phải khớp với @SerializedName("BookmarkType")
        public string BookmarkType { get; set; }
    }
}