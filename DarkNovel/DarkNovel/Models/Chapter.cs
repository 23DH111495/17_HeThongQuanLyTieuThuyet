using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int ChapterNumber { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }  // Add ?

        [Required]
        public string Content { get; set; }  // Keep without ? since it's Required

        public int WordCount { get; set; } = 0;
        public DateTime PublishDate { get; set; } = DateTime.Now;
        public bool IsPublished { get; set; } = true;
        public int UnlockPrice { get; set; } = 0;
        public int FreePreviewWords { get; set; } = 0;
        public bool IsEarlyAccess { get; set; } = false;
        public bool IsPremium { get; set; } = false;
        public long ViewCount { get; set; } = 0;

        [MaxLength(20)]
        public string? ModerationStatus { get; set; } = "Approved";  // Add ?

        public int? ModeratedBy { get; set; }
        public DateTime? ModerationDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("NovelId")]
        public virtual Novel? Novel { get; set; }  // Add ?
    }
}