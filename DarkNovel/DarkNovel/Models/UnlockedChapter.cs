using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    public class UnlockedChapter
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChapterId { get; set; }

        [MaxLength(20)]
        public string? UnlockMethod { get; set; } = "Coins";  // Add ?

        public int CoinsSpent { get; set; } = 0;
        public DateTime UnlockDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }  // Add ?

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }  // Add ?
    }
}