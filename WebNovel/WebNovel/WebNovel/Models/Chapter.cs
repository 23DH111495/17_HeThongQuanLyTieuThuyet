using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Chapter
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Novel selection is required")]
        [Display(Name = "Novel")]
        public int NovelId { get; set; }

        [Required(ErrorMessage = "Chapter number is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Chapter number must be greater than 0")]
        [Display(Name = "Chapter Number")]
        public int ChapterNumber { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Chapter Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Word Count")]
        public int WordCount { get; set; }

        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Published")]
        public bool IsPublished { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Unlock price must be 0 or greater")]
        [Display(Name = "Unlock Price (Coins)")]
        public int UnlockPrice { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Free preview words must be 0 or greater")]
        [Display(Name = "Free Preview Words")]
        public int FreePreviewWords { get; set; } = 0;

        [Display(Name = "Early Access")]
        public bool IsEarlyAccess { get; set; } = false;

        [Display(Name = "Premium Chapter")]
        public bool IsPremium { get; set; } = false;

        [Display(Name = "View Count")]
        public long ViewCount { get; set; } = 0;

        [StringLength(20)]
        [Display(Name = "Moderation Status")]
        public string ModerationStatus { get; set; } = "Approved";

        [Display(Name = "Moderated By")]
        public int? ModeratedBy { get; set; }

        [Display(Name = "Moderation Date")]
        public DateTime? ModerationDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }

        [ForeignKey("ModeratedBy")]
        public virtual User ModeratedByUser { get; set; }
    }
}