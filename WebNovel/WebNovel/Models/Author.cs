using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("PenName")]
        public string PenName { get; set; }

        [Column("Biography")]
        public string Biography { get; set; }

        public string Website { get; set; }
        public string SocialLinks { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerificationDate { get; set; }
        public int? VerifiedBy { get; set; }
        public string AuthorRank { get; set; }
        public int TotalNovels { get; set; }
        public long TotalViews { get; set; }
        public int TotalFollowers { get; set; }
        public string PayoutInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<Novel> Novels { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("VerifiedBy")]
        public virtual User VerifiedByUser { get; set; }
    }
}