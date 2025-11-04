using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class Author
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? PenName { get; set; }

        public string? Biography { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        public string? SocialLinks { get; set; }

        public bool? IsVerified { get; set; } = false;

        public DateTime? VerificationDate { get; set; }

        public int? VerifiedBy { get; set; }

        [MaxLength(50)]
        public string? AuthorRank { get; set; } = "Novice";

        public int? TotalNovels { get; set; } = 0;

        public long? TotalViews { get; set; } = 0;

        public int? TotalFollowers { get; set; } = 0;

        public string? PayoutInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("VerifiedBy")]
        public virtual User? VerifiedByUser { get; set; }

        public virtual ICollection<Novel>? Novels { get; set; }
    }
}
