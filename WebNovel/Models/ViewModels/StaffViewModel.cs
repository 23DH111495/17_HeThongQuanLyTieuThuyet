using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class StaffViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [StringLength(20)]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; }

        [StringLength(50)]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [StringLength(100)]
        [Display(Name = "Position")]
        public string Position { get; set; }

        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        [Display(Name = "Supervisor")]
        public int? SupervisorId { get; set; }

        [Display(Name = "Permissions")]
        public string Permissions { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        // Related user info
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string SupervisorName { get; set; }
    }
}