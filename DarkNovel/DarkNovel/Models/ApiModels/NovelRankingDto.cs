using System;
using System.Collections.Generic;

namespace DarkNovel.Models.ApiModels
{
    public class NovelRankingDto
    {
        public int Rank { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Synopsis { get; set; }
        public string Status { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalChapters { get; set; }
        public long ViewCount { get; set; }
        public long BookmarkCount { get; set; }
        public long WordCount { get; set; }
        public bool IsPremium { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> Genres { get; set; }
    }
}