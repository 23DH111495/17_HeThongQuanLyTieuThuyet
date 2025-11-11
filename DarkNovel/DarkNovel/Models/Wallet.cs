using DarkNovel.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Wallets")]
public class Wallet
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("CoinBalance")]
    public decimal CoinBalance { get; set; }   // <- decimal

    [Column("TotalCoinsEarned")]
    public decimal TotalCoinsEarned { get; set; }  // <- decimal

    [Column("TotalCoinsSpent")]
    public decimal TotalCoinsSpent { get; set; }   // <- decimal

    [Column("LastUpdated")]
    public DateTime LastUpdated { get; set; }

    [Column("Version")]
    public int Version { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
