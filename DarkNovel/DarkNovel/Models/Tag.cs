using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }  // Add ?

        [MaxLength(255)]
        public string? Description { get; set; }  // Add ?

        [MaxLength(7)]
        public string? Color { get; set; }  // Add ?

        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<NovelTag>? NovelTags { get; set; }  // Add ?
    }
}