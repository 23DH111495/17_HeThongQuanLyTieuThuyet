using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebNovel.Models
{
    [Bind(Exclude = "CoverImage")]
    public class Novel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [StringLength(200)]
        public string AlternativeTitle { get; set; }
        public string Slug { get; set; }
        [Required]
        public int AuthorId { get; set; }
        public string Synopsis { get; set; }
        [StringLength(500)]
        public string CoverImageUrl { get; set; }
        [StringLength(20)]
        public string Status { get; set; } = "Ongoing";
        public DateTime PublishDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        [StringLength(10)]
        public string Language { get; set; } = "EN";
        [StringLength(10)]
        public string OriginalLanguage { get; set; }
        [StringLength(20)]
        public string TranslationStatus { get; set; }
        public bool IsOriginal { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsWeeklyFeatured { get; set; } = false;
        public bool IsSliderFeatured { get; set; } = false;
        public long ViewCount { get; set; } = 0;
        public long BookmarkCount { get; set; } = 0;
        [Column(TypeName = "decimal")]
        public decimal AverageRating { get; set; } = 0.00m;
        public int TotalRatings { get; set; } = 0;
        public int TotalChapters { get; set; } = 0;
        public long WordCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsPremium { get; set; } = false;
        [StringLength(20)]
        public string ModerationStatus { get; set; } = "Pending";
        public int? ModeratedBy { get; set; }
        public DateTime? ModerationDate { get; set; }
        public string ModerationNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; }
        public virtual ICollection<NovelGenre> NovelGenres { get; set; }
        public virtual ICollection<NovelTag> NovelTags { get; set; }
        public virtual ICollection<Chapter> Chapters { get; set; }
        [NotMapped]
        public virtual List<Genre> Genres { get; set; } = new List<Genre>();



        public byte[] CoverImage { get; set; }
        [StringLength(100)]
        public string CoverImageContentType { get; set; }

        [StringLength(255)]
        public string CoverImageFileName { get; set; }
        // Helper method to check if novel has cover image
        public bool HasCoverImage => CoverImage != null && CoverImage.Length > 0;

    }

   
}