using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebNovel.Models.ViewModels;

namespace WebNovel.Models.ViewModels
{
    public class NovelDetailsViewModel
    {
        public string Slug { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string AlternativeTitle { get; set; }
        public string Synopsis { get; set; }
        public string CoverImageUrl { get; set; }
        public string Status { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Language { get; set; }
        public string OriginalLanguage { get; set; }
        public bool IsOriginal { get; set; }

        // Statistics
        public long ViewCount { get; set; }
        public long BookmarkCount { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalChapters { get; set; }
        public long WordCount { get; set; }
        public bool IsPremium { get; set; }

        // Author Information
        public AuthorViewModel Author { get; set; }

        // Rankings
        public int? CurrentRank { get; set; }

        // Genres and Tags
        public List<string> Genres { get; set; }
        public List<string> Tags { get; set; }

        // Recent Chapters
        public List<ChapterViewModel> RecentChapters { get; set; }

        // Reviews and Comments
        public List<ReviewViewModel> Reviews { get; set; }
        public List<CommentViewModel> Comments { get; set; }

        // User-specific data (if user is logged in)
        public bool IsBookmarked { get; set; }
        public decimal? UserRating { get; set; }
        public int? LastReadChapter { get; set; }

        // Calculated properties
        public string FormattedViewCount => FormatNumber(ViewCount);
        public string FormattedBookmarkCount => FormatNumber(BookmarkCount);
        public string FormattedWordCount => FormatNumber(WordCount);
        public string EstimatedReadingTime => $"~{WordCount / 200 / 60} hours"; // Assuming 200 words per minute
        public string TimeSinceLastUpdate => GetTimeAgo(LastUpdated);
        public string TimeSincePublished => GetTimeAgo(PublishDate);

        private string FormatNumber(long number)
        {
            if (number >= 1000000)
                return $"{number / 1000000.0:F1}M";
            if (number >= 1000)
                return $"{number / 1000.0:F1}K";
            return number.ToString();
        }

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            if (timeSpan.Days > 365)
                return $"{timeSpan.Days / 365} year{(timeSpan.Days / 365 > 1 ? "s" : "")} ago";
            if (timeSpan.Days > 30)
                return $"{timeSpan.Days / 30} month{(timeSpan.Days / 30 > 1 ? "s" : "")} ago";
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ago";
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ago";
            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")} ago";

            return "Just now";
        }
    }
}