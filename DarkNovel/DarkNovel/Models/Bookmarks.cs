using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    public class Bookmark
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ReaderId { get; set; }
        public int NovelId { get; set; }

        [MaxLength(100)]
        public string BookmarkType { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // --- SỬA LỖI: THÊM 2 DÒNG NÀY ---
        // Thuộc tính liên kết (Navigation Property)
        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }
    }
}