using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class NovelGenre
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int GenreId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("NovelId")]
        public virtual Novel? Novel { get; set; }  // Add ?

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }  // Add ?
    }
}