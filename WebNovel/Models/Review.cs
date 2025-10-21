using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ReaderId { get; set; }
        public int NovelId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public decimal Rating { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool IsApproved { get; set; }
        public string ModerationStatus { get; set; }
        public int? ModeratedBy { get; set; }
        public DateTime? ModerationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Reader Reader { get; set; }
        public virtual Novel Novel { get; set; }
    }
}