using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class Novel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }  // Add ?

        [MaxLength(200)]
        public string? AlternativeTitle { get; set; }  // Add ?

        public int AuthorId { get; set; }

        public string? Synopsis { get; set; }  // Add ?

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }  // Add ?

        public byte[]? CoverImage { get; set; }  // Add ?

        [MaxLength(100)]
        public string? CoverImageContentType { get; set; }  // Add ?

        [MaxLength(255)]
        public string? CoverImageFileName { get; set; }  // Add ?

        [MaxLength(20)]
        public string? Status { get; set; } = "Ongoing";  // Add ?

        public DateTime PublishDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [MaxLength(10)]
        public string? Language { get; set; } = "EN";  // Add ?

        [MaxLength(10)]
        public string? OriginalLanguage { get; set; }  // Add ?

        [MaxLength(20)]
        public string? TranslationStatus { get; set; }  // Add ?

        public bool IsOriginal { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsWeeklyFeatured { get; set; } = false;
        public bool IsSliderFeatured { get; set; } = false;

        public long ViewCount { get; set; } = 0;
        public long BookmarkCount { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0.00m;
        public int TotalRatings { get; set; } = 0;
        public int TotalChapters { get; set; } = 0;
        public long WordCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsPremium { get; set; } = false;

        [MaxLength(20)]
        public string? ModerationStatus { get; set; } = "Pending";  // Add ?

        public int? ModeratedBy { get; set; }
        public DateTime? ModerationDate { get; set; }

        public string? ModerationNotes { get; set; }  // Add ?

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Slug { get; set; }  // Add ?

        // Navigation Properties
        [ForeignKey("AuthorId")]
        public virtual Author? Author { get; set; }  // Add ?

        public virtual ICollection<Chapter>? Chapters { get; set; }  // Add ?
        public virtual ICollection<NovelGenre>? NovelGenres { get; set; }  // Add ?
        public virtual ICollection<NovelTag>? NovelTags { get; set; }  // Add ?
    }
}