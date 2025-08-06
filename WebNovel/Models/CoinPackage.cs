using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class CoinPackage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Package name must be between 3 and 100 characters")]
        [Display(Name = "Package Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Coin amount is required")]
        [Range(1, 999999, ErrorMessage = "Coin amount must be between 1 and 999,999")]
        [Display(Name = "Base Coins")]
        public int CoinAmount { get; set; }

        [Range(0, 999999, ErrorMessage = "Bonus coins must be between 0 and 999,999")]
        [Display(Name = "Bonus Coins")]
        public int BonusCoins { get; set; } = 0;

        [Required(ErrorMessage = "USD price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
        [Display(Name = "Price (USD)")]
        public decimal PriceUSD { get; set; }

        [Range(0, 99999999999.99, ErrorMessage = "VND price must be between 0 and 99,999,999,999.99")]
        [Display(Name = "Price (VND)")]
        public decimal? PriceVND { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; } = false;

        [Range(1, 9999, ErrorMessage = "Sort order must be between 1 and 9,999")]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 1;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Calculated property - not stored in database
        [NotMapped]
        [Display(Name = "Total Coins")]
        public int TotalCoins => CoinAmount + BonusCoins;

        // Navigation properties
        public virtual ICollection<CoinPurchaseHistory> PurchaseHistory { get; set; }

        // Constructor
        public CoinPackage()
        {
            PurchaseHistory = new HashSet<CoinPurchaseHistory>();
        }
    }
}