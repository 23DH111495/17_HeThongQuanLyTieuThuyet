using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class AuthorFollower
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public int ReaderId { get; set; }
        public DateTime FollowDate { get; set; } = DateTime.Now;
        public bool NotificationsEnabled { get; set; } = true;

        // Navigation Properties
        [ForeignKey("AuthorId")]
        public virtual Author? Author { get; set; }  // Add ?

        [ForeignKey("ReaderId")]
        public virtual Reader? Reader { get; set; }  // Add ?
    }
}