using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("PromoCodeUsage")]
    public class PromoCodeUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PromoCodeId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Mặc định là 0, được gán giá trị khi loại promo là 'FreeCoins'
        [Column(TypeName = "int")]
        public int CoinsReceived { get; set; } = 0;

        // Mặc định là 0, được gán giá trị khi loại promo là 'Discount'
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountReceived { get; set; } = 0;

        [Required]
        public DateTime UsedDate { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("PromoCodeId")]
        public virtual PromoCode PromoCode { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // --- Constructor ---
        public PromoCodeUsage()
        {
            UsedDate = DateTime.UtcNow;
        }
    }
}