using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class NovelTag
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int TagId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("NovelId")]
        public virtual Novel? Novel { get; set; }  // Add ?

        [ForeignKey("TagId")]
        public virtual Tag? Tag { get; set; }  // Add ?
    }
}