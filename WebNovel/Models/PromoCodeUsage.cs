using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class PromoCodeUsage
    {
        public int Id { get; set; }
        public int PromoCodeId { get; set; }
        public int UserId { get; set; }

        public int CoinsReceived { get; set; } = 0;

        //[Column(TypeName = "decimal(10,2)")]
        //[Column(TypeName = "money")]
        public decimal DiscountReceived { get; set; } = 0;

        public DateTime UsedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual PromoCode PromoCode { get; set; }
        public virtual User User { get; set; }
    }
}