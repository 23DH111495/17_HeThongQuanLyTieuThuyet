using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public int UserId { get; set; }

        public int AdminLevel { get; set; } = 1; // 1=Junior Admin, 2=Senior Admin, 3=Super Admin

        public string SpecialPermissions { get; set; } // JSON format for admin permissions

        public DateTime? LastAdminAction { get; set; }

        public bool TwoFactorEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}