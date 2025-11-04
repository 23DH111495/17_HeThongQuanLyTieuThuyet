using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class UserRoleAssignment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public int? AssignedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }  // Add ?

        [ForeignKey("RoleId")]
        public virtual UserRole? UserRole { get; set; }  // Add ?

        [ForeignKey("AssignedBy")]
        public virtual User? AssignedByUser { get; set; }  // Add ?
    }
}