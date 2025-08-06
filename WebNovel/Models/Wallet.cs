using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTopUp { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarned { get; set; } = 0.00m;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}