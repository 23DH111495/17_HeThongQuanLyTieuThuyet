using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class AdminViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Admin Level")]
        [Range(1, 3)]
        public int AdminLevel { get; set; }

        [Display(Name = "Special Permissions")]
        public string SpecialPermissions { get; set; }

        [Display(Name = "Last Admin Action")]
        public DateTime? LastAdminAction { get; set; }

        [Display(Name = "Two Factor Enabled")]
        public bool TwoFactorEnabled { get; set; }

        // Related user info
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        [Display(Name = "Admin Level Name")]
        public string AdminLevelName
        {
            get
            {
                switch (AdminLevel)
                {
                    case 1: return "Junior Admin";
                    case 2: return "Senior Admin";
                    case 3: return "Super Admin";
                    default: return "Unknown";
                }
            }
        }

    }
}