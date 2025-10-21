using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerInitials { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public decimal Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }

        public string TimeAgo => GetTimeAgo(CreatedDate);

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            if (timeSpan.Days > 7)
                return $"{timeSpan.Days / 7} week{(timeSpan.Days / 7 > 1 ? "s" : "")} ago";
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ago";
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ago";

            return "Recently";
        }
    }
}