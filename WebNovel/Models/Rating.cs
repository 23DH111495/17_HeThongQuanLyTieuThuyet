using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReaderId { get; set; }

        [Required]
        public int NovelId { get; set; }

        [Column("Rating")]
        [Required]
        [Range(1.0, 5.0)]
        public decimal RatingValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ReaderId")]
        public virtual Reader Reader { get; set; }

        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }
    }
}