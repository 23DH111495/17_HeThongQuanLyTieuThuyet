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
        [Key] // Make UserId the primary key
        [ForeignKey("User")]
        public int UserId { get; set; }

        // Remove the separate Id property or make it non-key
        // public int Id { get; set; } // Remove this line

        //[Column(TypeName = "decimal(18,2)")]
        public decimal CoinBalance { get; set; }= 0.00m;

        //[Column(TypeName = "decimal(18,2)")]
        public decimal TotalTopUp { get; set; } = 0.00m;

        //[Column(TypeName = "decimal(18,2)")]
        public decimal TotalCoinsSpent { get; set; } = 0.00m;

        //[Column(TypeName = "decimal(18,2)")]
        public decimal TotalCoinsEarned { get; set; } = 0.00m;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual User User { get; set; }
    }
}