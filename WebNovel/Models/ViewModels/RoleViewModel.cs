using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role Name")]
        public string Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Permissions")]
        public string Permissions { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
    }
}