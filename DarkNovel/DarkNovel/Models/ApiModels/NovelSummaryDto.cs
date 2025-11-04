using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DarkNovel.Models.ApiModels
{
    public class NovelSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Synopsis { get; set; }
        public string CoverImageUrl { get; set; }
        public string Status { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalChapters { get; set; }
        public long ViewCount { get; set; }
        public bool IsPremium { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> Genres { get; set; }
    }
}