using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class UserRole
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public string Permissions { get; set; } // JSON

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new HashSet<UserRoleAssignment>();
    }
}