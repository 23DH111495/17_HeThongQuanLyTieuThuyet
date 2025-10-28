namespace DarkNovel.Models
{
    public class CoinPackage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CoinAmount { get; set; }
        public int BonusCoins { get; set; }
        public decimal PriceUSD { get; set; }
        public decimal PriceVND { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
