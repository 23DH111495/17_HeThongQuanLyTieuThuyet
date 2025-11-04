using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("NovelId")]
        public int? NovelId { get; set; }

        [Column("ChapterId")]
        public int? ChapterId { get; set; }

        [Column("ParentCommentId")]
        public int? ParentCommentId { get; set; }

        [Column("Content")]
        public string? Content { get; set; }

        [Column("LikeCount")]
        public int LikeCount { get; set; } = 0;

        [Column("DislikeCount")]
        public int DislikeCount { get; set; } = 0;

        [Column("IsApproved")]
        public bool IsApproved { get; set; } = true;

        [Column("ModerationStatus")]
        public string? ModerationStatus { get; set; } = "Approved";

        [Column("ModeratedBy")]
        public int? ModeratedBy { get; set; }

        [Column("ModerationDate")]
        public DateTime? ModerationDate { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("VotedUserIds")]
        public string? VotedUserIds { get; set; }

        [Column("LikedUserIds")]
        public string? LikedUserIds { get; set; }

        [Column("DislikedUserIds")]
        public string? DislikedUserIds { get; set; }

        [JsonIgnore]
        [Column("CommentImage")]
        public byte[]? CommentImage { get; set; }

        [JsonIgnore]
        [Column("CommentImageContentType")]
        public string? CommentImageContentType { get; set; }

        [JsonIgnore]
        [Column("CommentImageFileName")]
        public string? CommentImageFileName { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("NovelId")]
        public virtual Novel? Novel { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        [ForeignKey("ModeratedBy")]
        public virtual User? Moderator { get; set; }

        public virtual ICollection<Comment>? Replies { get; set; }
    }
}