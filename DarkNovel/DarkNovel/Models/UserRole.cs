using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class UserRole
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }  // Add ?

        [MaxLength(255)]
        public string? Description { get; set; }  // Add ?

        public string? Permissions { get; set; }  // Add ?

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<UserRoleAssignment>? UserRoleAssignments { get; set; }  // Add ?
    }
}