using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class ReadingProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReaderId { get; set; }

        [Required]
        public int NovelId { get; set; }

        public int? LastReadChapterId { get; set; }

        public int LastReadChapterNumber { get; set; } = 0;

        public DateTime LastReadDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string ReadingStatus { get; set; } = "Reading";

        public int TotalReadTime { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ReaderId")]
        public virtual Reader Reader { get; set; }

        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }

        [ForeignKey("LastReadChapterId")]
        public virtual Chapter LastReadChapter { get; set; }
    }
}