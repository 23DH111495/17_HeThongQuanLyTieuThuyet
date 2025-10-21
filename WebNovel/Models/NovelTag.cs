using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class NovelTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NovelId { get; set; }

        [Required]
        public int TagId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

    }
}