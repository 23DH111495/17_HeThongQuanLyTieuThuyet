using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("CoinPurchaseHistory")]
    public class CoinPurchaseHistory
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("PackageId")]
        public int PackageId { get; set; }

        [Column("CoinsPurchased")]
        public int CoinsPurchased { get; set; }

        [Column("BonusCoins")]
        public int BonusCoins { get; set; }

        [Column("PricePaid")]
        public decimal PricePaid { get; set; }

        [Column("Currency")]
        public string Currency { get; set; }

        [Column("PaymentMethod")]
        public string? PaymentMethod { get; set; }

        [Column("PaymentGateway")]
        public string? PaymentGateway { get; set; }

        [Column("TransactionId")]
        public string? TransactionId { get; set; }

        [Column("GatewayTransactionId")]
        public string? GatewayTransactionId { get; set; }

        [Column("PaymentStatus")]
        public string PaymentStatus { get; set; }

        [Column("PaymentDate")]
        public DateTime? PaymentDate { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PackageId")]
        public virtual CoinPackage CoinPackage { get; set; }
    }
}
