using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class CoinTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string TransactionType { get; set; } // Purchase, Spend, Refund, Gift, Bonus, DailyLogin, Achievement

        public int Amount { get; set; } // Positive for gains, negative for spending
        public int BalanceBefore { get; set; }
        public int BalanceAfter { get; set; }

        public int? RelatedChapterId { get; set; }
        public int? RelatedNovelId { get; set; }
        public int? RelatedPurchaseId { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Chapter RelatedChapter { get; set; }
        public virtual Novel RelatedNovel { get; set; }
        public virtual CoinPurchaseHistory RelatedPurchase { get; set; }
    }
}