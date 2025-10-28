using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public int UserId { get; set; }

        [StringLength(20)]
        [Index(IsUnique = true)]
        public string EmployeeId { get; set; }

        [StringLength(50)]
        public string Department { get; set; } // Content, Support, Marketing, Technical

        [StringLength(100)]
        public string Position { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        public int? SupervisorId { get; set; }

        public string Permissions { get; set; } // JSON format for specific permissions

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual Staff Supervisor { get; set; }

        public virtual ICollection<Staff> Subordinates { get; set; } = new HashSet<Staff>();
    }
}