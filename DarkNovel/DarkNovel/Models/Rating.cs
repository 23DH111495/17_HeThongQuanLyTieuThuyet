using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("Ratings")]
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReaderId { get; set; }

        [Required]
        public int NovelId { get; set; }

        // ✅ Property is named "RatingValue" to avoid conflict with class name "Rating"
        // ✅ Maps to database column "Rating"
        [Column("Rating")]
        [Required]
        [Range(1.0, 5.0)]
        public decimal RatingValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ReaderId")]
        public virtual Reader? Reader { get; set; }

        [ForeignKey("NovelId")]
        public virtual Novel? Novel { get; set; }
    }
}