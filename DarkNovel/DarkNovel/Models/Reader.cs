using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class Reader
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsPremium { get; set; } = false;
        public DateTime? PremiumExpiryDate { get; set; }

        public string? ReadingPreferences { get; set; }  // Add ?
        public string? FavoriteGenres { get; set; }  // Add ?
        public string? NotificationSettings { get; set; }  // Add ?

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }  // Add ?

        public virtual ICollection<AuthorFollower>? FollowedAuthors { get; set; }  // Add ?

        public virtual ICollection<Rating>? Ratings { get; set; }
    }
}