using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class GiftTransaction
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int? NovelId { get; set; }

        [Required, StringLength(50)]
        public string GiftType { get; set; } // DirectGift, NovelSupport, AuthorTip

        [Range(1, int.MaxValue)]
        public int CoinAmount { get; set; }

        [StringLength(500)]
        public string Message { get; set; }

        public bool IsAnonymous { get; set; } = false;

        [StringLength(20)]
        public string Status { get; set; } = "Completed"; // Pending, Completed, Cancelled

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User Sender { get; set; }
        public virtual User Recipient { get; set; }
        public virtual Novel Novel { get; set; }
    }
}