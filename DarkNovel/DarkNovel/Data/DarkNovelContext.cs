using Azure;
using DarkNovel.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DarkNovel.Data
{
    public class DarkNovelContext : DbContext
    {
        public DarkNovelContext(DbContextOptions<DarkNovelContext> options) : base(options) { }
        public DbSet<Novel> Novels { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<PromoCodeUsage> PromoCodeUsage { get; set; }
        public DbSet<NovelTag> NovelTags { get; set; }
        public DbSet<NovelGenre> NovelGenres { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<UnlockedChapter> UnlockedChapters { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AuthorFollower> AuthorFollowers { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<CoinTransaction> CoinTransactions { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CoinPackage> CoinPackages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }//DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
        public DbSet<Bookmark> Bookmarks { get; set; } 


        public DbSet<CoinPurchaseHistory> CoinPurchaseHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PromoCodeUsage>(entity =>
            {
                entity.HasIndex(e => new { e.PromoCodeId, e.UserId })
                      .IsUnique();
            });

            modelBuilder.Entity<CoinPackage>(entity =>
            {
                entity.Property(e => e.PriceUSD).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PriceVND).HasColumnType("decimal(15, 2)");
            });

            modelBuilder.Entity<CoinPurchaseHistory>(entity =>
            {
                entity.Property(e => e.PricePaid).HasColumnType("decimal(18, 2)");
            });
        }
    }
}
