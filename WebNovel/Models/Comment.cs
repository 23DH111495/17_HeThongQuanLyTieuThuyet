using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? NovelId { get; set; }

        public int? ChapterId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public int LikeCount { get; set; } = 0;

        public int DislikeCount { get; set; } = 0;

        public bool IsApproved { get; set; } = true;
        public string LikedUserIds { get; set; }
        public string DislikedUserIds { get; set; }

        // Image support properties
        public byte[] CommentImage { get; set; }

        [StringLength(100)]
        public string CommentImageContentType { get; set; }

        [StringLength(255)]
        public string CommentImageFileName { get; set; }

        [StringLength(20)]
        public string ModerationStatus { get; set; } = "Approved";

        public int? ModeratedBy { get; set; }

        public DateTime? ModerationDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment ParentComment { get; set; }

        [ForeignKey("ModeratedBy")]
        public virtual User Moderator { get; set; }

        public virtual ICollection<Comment> Replies { get; set; }

        // Helper property to check if comment has an image
        [NotMapped]
        public bool HasImage => CommentImage != null && CommentImage.Length > 0;
    }
}