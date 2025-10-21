using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                return Name?.StartsWith("#") == true ? Name.Substring(1) : Name;
            }
        }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(7)]
        public string Color { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}