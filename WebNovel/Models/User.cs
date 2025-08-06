using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebNovel.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Username { get; set; }
        [Required, StringLength(100)]
        public string Email { get; set; }
        [Required, StringLength(255)]
        public string PasswordHash { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(255)]
        public string ProfilePicture { get; set; } // Keep for backward compatibility or URL fallback

        // Binary profile picture data (NEW)
        public byte[] ProfilePictureData { get; set; }
        [StringLength(100)]
        public string ProfilePictureContentType { get; set; }
        [StringLength(255)]
        public string ProfilePictureFileName { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        //public virtual ICollection<UserRoleAssignment> RoleAssignments { get; set; }
        public virtual ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new HashSet<UserRoleAssignment>();
        public virtual Reader Reader { get; set; }
        public virtual Author Author { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Admin Admin { get; set; }
    }
}