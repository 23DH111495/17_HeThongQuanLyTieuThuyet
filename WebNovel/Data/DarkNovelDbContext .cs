using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Data
{
    public class DarkNovelDbContext : DbContext
    {
        public DarkNovelDbContext() : base("DefaultConnection")
        {

        }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Genre> Genres { get; set; }

        public DbSet<Novel> Novels { get; set; }
        public DbSet<NovelGenre> NovelGenres { get; set; }
        public DbSet<NovelTag> NovelTags { get; set; }
        public DbSet<Chapter> Chapters { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<CoinPackage> CoinPackages { get; set; }
        public DbSet<CoinPurchaseHistory> PurchaseHistories { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Ranking> Rankings { get; set; }
        public DbSet<ReadingProgress> ReadingProgress { get; set; }
        public DbSet<AuthorFollower> AuthorFollowers { get; set; }

        // Wallet and Coin System
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<CoinTransaction> CoinTransactions { get; set; }
        public DbSet<UnlockedChapter> UnlockedChapters { get; set; }
        public DbSet<GiftTransaction> GiftTransactions { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOptional(u => u.Wallet)
                .WithRequired(w => w.User);


            // Configure UserRoleAssignment relationships
            modelBuilder.Entity<UserRoleAssignment>()
                .HasRequired(ura => ura.User)
                .WithMany(u => u.UserRoleAssignments)
                .HasForeignKey(ura => ura.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserRoleAssignment>()
                .HasRequired(ura => ura.UserRole)
                .WithMany(ur => ur.UserRoleAssignments)
                .HasForeignKey(ura => ura.RoleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserRoleAssignment>()
                .HasOptional(ura => ura.AssignedByUser)
                .WithMany()
                .HasForeignKey(ura => ura.AssignedBy)
                .WillCascadeOnDelete(false);

            // Configure unique constraint for UserRoleAssignment
            modelBuilder.Entity<UserRoleAssignment>()
                .HasIndex(ura => new { ura.UserId, ura.RoleId })
                .IsUnique();

            // Configure Reader relationship
            modelBuilder.Entity<Reader>()
                .HasKey(r => r.UserId);

            modelBuilder.Entity<Reader>()
                .HasRequired(r => r.User)
                .WithOptional(u => u.Reader);

            // Configure Staff relationship
            modelBuilder.Entity<Staff>()
                .HasKey(s => s.UserId);

            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.User)
                .WithOptional(u => u.Staff)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Staff>()
                .HasOptional(s => s.Supervisor)
                .WithMany(s => s.Subordinates)
                .HasForeignKey(s => s.SupervisorId)
                .WillCascadeOnDelete(false);

            // Configure Admin relationship
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.UserId);

            modelBuilder.Entity<Admin>()
                .HasRequired(a => a.User)
                .WithOptional(u => u.Admin);

            // Configure AuthorFollower relationships                                                   
            modelBuilder.Entity<Author>()
                .HasKey(a => a.UserId);

            modelBuilder.Entity<Author>()
                .HasRequired(a => a.User)
                .WithOptional(u => u.Author);

            modelBuilder.Entity<Author>()
                .HasOptional(a => a.VerifiedByUser)
                .WithMany()
                .HasForeignKey(a => a.VerifiedBy);

            modelBuilder.Entity<AuthorFollower>()
                .HasRequired(af => af.Reader)
                .WithMany(r => r.FollowedAuthors)
                .HasForeignKey(af => af.ReaderId)
                .WillCascadeOnDelete(false);

            // Configure unique constraint for AuthorFollower
            modelBuilder.Entity<AuthorFollower>()
                .HasIndex(af => new { af.AuthorId, af.ReaderId })
                .IsUnique();

            // Configure Novel
            modelBuilder.Entity<Novel>()
                .Property(n => n.AverageRating)
                .HasPrecision(3, 2);

            // Configure Novel relationships
            modelBuilder.Entity<Novel>()
                .HasRequired(n => n.Author)
                .WithMany(a => a.Novels)
                .HasForeignKey(n => n.AuthorId);

            modelBuilder.Entity<NovelGenre>()
                .HasRequired(ng => ng.Novel)
                .WithMany(n => n.NovelGenres)
                .HasForeignKey(ng => ng.NovelId);

            modelBuilder.Entity<NovelGenre>()
                .HasRequired(ng => ng.Genre)
                .WithMany(g => g.NovelGenres)
                .HasForeignKey(ng => ng.GenreId);

            modelBuilder.Entity<Chapter>()
                .HasRequired(c => c.Novel)
                .WithMany(n => n.Chapters)
                .HasForeignKey(c => c.NovelId);

            // Configure unique constraints
            modelBuilder.Entity<NovelGenre>()
                .HasIndex(ng => new { ng.NovelId, ng.GenreId })
                .IsUnique();

            modelBuilder.Entity<Chapter>()
                .HasIndex(c => new { c.NovelId, c.ChapterNumber })
                .IsUnique();

            // Configure CoinPackage (SINGLE CONFIGURATION - removed duplicate)
            modelBuilder.Entity<CoinPackage>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<CoinPackage>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<CoinPackage>()
                .Property(c => c.PriceUSD)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CoinPackage>()
                .Property(c => c.PriceVND)
                .HasPrecision(15, 2);

            // Configure CoinPurchaseHistory
            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<CoinPurchaseHistory>()
                .Property(p => p.PricePaid)
                .HasPrecision(10, 2);

            // Configure CoinPurchaseHistory relationships
            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasRequired(p => p.Package)
                .WithMany(c => c.PurchaseHistory)
                .HasForeignKey(p => p.PackageId)
                .WillCascadeOnDelete(false);  

            // Index configurations for better performance
            modelBuilder.Entity<CoinPackage>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<CoinPackage>()
                .HasIndex(c => c.IsActive);

            modelBuilder.Entity<CoinPackage>()
                .HasIndex(c => c.SortOrder);

            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasIndex(p => p.CreatedAt);

            modelBuilder.Entity<CoinPurchaseHistory>()
                .HasIndex(p => p.PaymentStatus);

            // Configure PromoCodeUsage  
            modelBuilder.Entity<PromoCodeUsage>()
                .Property(p => p.DiscountReceived)
                .HasColumnType("decimal")
                .HasPrecision(10, 2);

            // Fix table naming - Entity Framework pluralizes by default
            modelBuilder.Entity<PromoCodeUsage>().ToTable("PromoCodeUsage");

            // Configure PromoCode relationships
            modelBuilder.Entity<PromoCode>()
                .HasOptional(p => p.Creator)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .WillCascadeOnDelete(false);

            // Configure PromoCodeUsage decimal property
            modelBuilder.Entity<PromoCodeUsage>()
                .Property(p => p.DiscountReceived)
                .HasColumnType("decimal")
                .HasPrecision(10, 2);

            // Configure PromoCodeUsage relationships
            modelBuilder.Entity<PromoCodeUsage>()
                .HasRequired(pu => pu.PromoCode)
                .WithMany(p => p.UsageHistory)
                .HasForeignKey(pu => pu.PromoCodeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PromoCodeUsage>()
                .HasRequired(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserId)
                .WillCascadeOnDelete(false);

            // Configure ReadingProgress relationships
                        modelBuilder.Entity<ReadingProgress>().ToTable("ReadingProgress");

            modelBuilder.Entity<ReadingProgress>()
                .HasRequired(rp => rp.Reader)
                .WithMany()
                .HasForeignKey(rp => rp.ReaderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ReadingProgress>()
                .HasRequired(rp => rp.Novel)
                .WithMany()
                .HasForeignKey(rp => rp.NovelId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ReadingProgress>()
                .HasOptional(rp => rp.LastReadChapter)
                .WithMany()
                .HasForeignKey(rp => rp.LastReadChapterId)
                .WillCascadeOnDelete(false);

            // Configure Wallet
            //modelBuilder.Entity<Wallet>()
            //    .HasKey(w => w.Id);

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .Property(w => w.CoinBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.TotalTopUp)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.TotalCoinsSpent)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.TotalCoinsEarned)
                .HasPrecision(18, 2);

            // Configure CoinTransaction
            modelBuilder.Entity<CoinTransaction>()
                .HasKey(ct => ct.Id);

            modelBuilder.Entity<CoinTransaction>()
                .HasRequired(ct => ct.User)
                .WithMany()
                .HasForeignKey(ct => ct.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CoinTransaction>()
                .HasOptional(ct => ct.RelatedChapter)
                .WithMany()
                .HasForeignKey(ct => ct.RelatedChapterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CoinTransaction>()
                .HasOptional(ct => ct.RelatedNovel)
                .WithMany()
                .HasForeignKey(ct => ct.RelatedNovelId)
                .WillCascadeOnDelete(false);

            // Configure UnlockedChapter
            modelBuilder.Entity<UnlockedChapter>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UnlockedChapter>()
                .HasRequired(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UnlockedChapter>()
                .HasRequired(uc => uc.Chapter)
                .WithMany()
                .HasForeignKey(uc => uc.ChapterId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UnlockedChapter>()
                .HasIndex(uc => new { uc.UserId, uc.ChapterId })
                .IsUnique();

            // Configure Review with decimal precision
            modelBuilder.Entity<Review>()
                .Property(r => r.Rating)
                .HasPrecision(2, 1);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Reader)
                .WithMany()
                .HasForeignKey(r => r.ReaderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Novel)
                .WithMany()
                .HasForeignKey(r => r.NovelId)
                .WillCascadeOnDelete(true);

            // Configure Rating with decimal precision
            modelBuilder.Entity<Rating>()
                .Property(r => r.RatingValue)
                .HasPrecision(2, 1);

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.Reader)
                .WithMany()
                .HasForeignKey(r => r.ReaderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.Novel)
                .WithMany()
                .HasForeignKey(r => r.NovelId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.ReaderId, r.NovelId })
                .IsUnique();

            // Configure Bookmark
            modelBuilder.Entity<Bookmark>()
                .HasRequired(b => b.Reader)
                .WithMany()
                .HasForeignKey(b => b.ReaderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Bookmark>()
                .HasRequired(b => b.Novel)
                .WithMany()
                .HasForeignKey(b => b.NovelId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Bookmark>()
                .HasIndex(b => new { b.ReaderId, b.NovelId })
                .IsUnique();

            // Unique constraint - one reading progress per user per novel
            modelBuilder.Entity<ReadingProgress>()
                .HasIndex(rp => new { rp.ReaderId, rp.NovelId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}