using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class ChapterReadViewModel
    {
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string ChapterTitle { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public long ViewCount { get; set; }

        public int NovelId { get; set; }
        public string NovelTitle { get; set; }

        public string AuthorName { get; set; }
        public int AuthorId { get; set; }

        public int? PreviousChapter { get; set; }
        public int? NextChapter { get; set; }
        public int TotalChapters { get; set; }

        // Computed properties
        public string FormattedViewCount
        {
            get
            {
                if (ViewCount >= 1000000)
                    return $"{ViewCount / 1000000.0:F1}M";
                if (ViewCount >= 1000)
                    return $"{ViewCount / 1000.0:F1}K";
                return ViewCount.ToString();
            }
        }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - PublishDate;
                if (timeSpan.Days > 0)
                    return $"{timeSpan.Days}d ago";
                if (timeSpan.Hours > 0)
                    return $"{timeSpan.Hours}h ago";
                if (timeSpan.Minutes > 0)
                    return $"{timeSpan.Minutes}m ago";
                return "Just now";
            }
        }
    }
}