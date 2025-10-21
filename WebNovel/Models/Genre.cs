using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        [StringLength(50)]
        public string IconClass { get; set; }

        [StringLength(7)]
        public string ColorCode { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<NovelGenre> NovelGenres { get; set; }

        public Genre()
        {
            IsActive = true;
            CreatedAt = DateTime.Now;
            NovelGenres = new HashSet<NovelGenre>();
        }
    }
}