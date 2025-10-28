using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class UnlockedChapter
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChapterId { get; set; }

        [StringLength(20)]
        public string UnlockMethod { get; set; } = "Coins"; // Coins, Premium, Free, Gift

        public int CoinsSpent { get; set; } = 0;
        public DateTime UnlockDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Chapter Chapter { get; set; }
    }
}