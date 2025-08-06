using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.ViewModels
{
    public class NovelViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(200)]
        [Display(Name = "Alternative Title")]
        public string AlternativeTitle { get; set; }

        [Required]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }

        [Display(Name = "Synopsis")]
        public string Synopsis { get; set; }

        [StringLength(500)]
        [Display(Name = "Cover Image URL")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Ongoing";

        [Display(Name = "Language")]
        public string Language { get; set; } = "EN";

        [Display(Name = "Is Premium")]
        public bool IsPremium { get; set; }

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Selected Genres")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        // For display purposes
        public string AuthorName { get; set; }
        public string GenresString { get; set; }
        public int ChapterCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}