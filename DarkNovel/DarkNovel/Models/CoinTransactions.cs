using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("CoinTransactions")]
    public class CoinTransaction
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("TransactionType")]
        public string TransactionType { get; set; }

        [Column("Amount")]
        public decimal Amount { get; set; }

        [Column("BalanceBefore")]
        public decimal BalanceBefore { get; set; }

        [Column("BalanceAfter")]
        public decimal BalanceAfter { get; set; }

        [Column("RelatedChapterId")]
        public int? RelatedChapterId { get; set; }

        [Column("RelatedNovelId")]
        public int? RelatedNovelId { get; set; }

        [Column("RelatedPurchaseId")]
        public int? RelatedPurchaseId { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("ReferenceId")]
        public string? ReferenceId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
