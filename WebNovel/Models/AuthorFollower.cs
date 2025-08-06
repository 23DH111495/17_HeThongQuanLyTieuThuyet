using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class AuthorFollower
    {
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int ReaderId { get; set; }

        public DateTime FollowDate { get; set; } = DateTime.Now;

        public bool NotificationsEnabled { get; set; } = true;

        // Navigation properties
        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; }

        [ForeignKey("ReaderId")]
        public virtual Reader Reader { get; set; }
    }
}