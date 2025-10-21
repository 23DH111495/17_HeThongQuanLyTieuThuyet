using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class ReaderViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Premium Member")]
        public bool IsPremium { get; set; }

        [Display(Name = "Premium Expiry")]
        public DateTime? PremiumExpiryDate { get; set; }

        [Display(Name = "Reading Preferences")]
        public string ReadingPreferences { get; set; }

        [Display(Name = "Favorite Genres")]
        public string FavoriteGenres { get; set; }

        [Display(Name = "Notification Settings")]
        public string NotificationSettings { get; set; }

        // Related user info
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}