using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    [Table("CoinPurchaseHistory")]
    public class CoinPurchaseHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int CoinsPurchased { get; set; }
        public int BonusCoins { get; set; } = 0;

        public decimal PricePaid { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "VNĐ";

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string PaymentGateway { get; set; }

        [StringLength(100)]
        public string TransactionId { get; set; }

        [StringLength(100)]
        public string GatewayTransactionId { get; set; }

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        public DateTime? PaymentDate { get; set; }
        public DateTime? RefundDate { get; set; }

        [StringLength(255)]
        public string RefundReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public int TotalCoinsReceived => CoinsPurchased + BonusCoins;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual CoinPackage Package { get; set; }
    }
}