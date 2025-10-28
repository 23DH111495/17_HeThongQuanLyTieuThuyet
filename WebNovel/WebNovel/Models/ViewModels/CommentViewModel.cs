using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CommenterName { get; set; }
        public string CommenterInitials { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public int? ParentCommentId { get; set; }

        // Image support properties
        public bool HasImage { get; set; }
        public string ImageContentType { get; set; }
        public string ImageFileName { get; set; }

        // CRITICAL: Ensure these are properly initialized
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool HasUserVoted { get; set; }
        public string UserVoteType { get; set; } // "like", "dislike", or null
        public int ReplyDepth { get; set; } = 0;
        public string LikedUserIds { get; set; }
        public string DislikedUserIds { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();

        // Helper property for display
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedDate;
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";
                return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
            }
        }

        // Helper property to check if we should allow more replies
        public bool CanReply => ReplyDepth < 5; // Limit to 5 levels deep

        // Helper property for CSS classes based on depth
        public string ReplyClass => ReplyDepth > 0 ? $"reply reply-depth-{Math.Min(ReplyDepth, 3)}" : "";

        public int ReplyCount => Replies?.Count ?? 0;

        // Helper property for image URL
        public string ImageUrl => HasImage ? $"/Book/GetCommentImage/{Id}" : null;
    }
}