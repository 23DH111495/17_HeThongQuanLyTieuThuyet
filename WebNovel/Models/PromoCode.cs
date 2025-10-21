using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class PromoCode
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required, StringLength(50)]
        public string PromoType { get; set; } // FreeCoins, DiscountPercent, DiscountFixed

        [Required]
        public int Value { get; set; }

        public int? MaxUses { get; set; }
        public int UsedCount { get; set; } = 0;

        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime? ValidUntil { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("Creator")]
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public bool IsExpired => ValidUntil.HasValue && ValidUntil < DateTime.Now;

        [NotMapped]
        public bool IsMaxUsesReached => MaxUses.HasValue && UsedCount >= MaxUses;

        // Navigation properties
        public virtual User Creator { get; set; }
        public virtual ICollection<PromoCodeUsage> UsageHistory { get; set; }
    }
}