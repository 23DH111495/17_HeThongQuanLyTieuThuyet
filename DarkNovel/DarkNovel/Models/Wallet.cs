using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("Wallets")]
    public class Wallet
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("CoinBalance")]
        public int CoinBalance { get; set; }

        [Column("TotalCoinsEarned")]
        public int TotalCoinsEarned { get; set; }

        [Column("TotalCoinsSpent")]
        public int TotalCoinsSpent { get; set; }

        [Column("LastUpdated")]
        public DateTime LastUpdated { get; set; }

        [Column("Version")]
        public int Version { get; set; }

        // Navigation property (tùy chọn nhưng nên có)
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
