using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Reader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public int UserId { get; set; }

        public bool IsPremium { get; set; } = false;

        public DateTime? PremiumExpiryDate { get; set; }

        public string ReadingPreferences { get; set; } // JSON format for preferences

        public string FavoriteGenres { get; set; } // JSON array of genre IDs

        public string NotificationSettings { get; set; } // JSON format for notification preferences

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<AuthorFollower> FollowedAuthors { get; set; } = new HashSet<AuthorFollower>();
    }
}