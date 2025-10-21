namespace WebNovel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Authors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Bio = c.String(),
                        ProfileImageUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Novels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        AlternativeTitle = c.String(maxLength: 200),
                        AuthorId = c.Int(nullable: false),
                        Synopsis = c.String(),
                        CoverImageUrl = c.String(maxLength: 500),
                        Status = c.String(maxLength: 20),
                        PublishDate = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        Language = c.String(maxLength: 10),
                        OriginalLanguage = c.String(maxLength: 10),
                        TranslationStatus = c.String(maxLength: 20),
                        IsOriginal = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        IsWeeklyFeatured = c.Boolean(nullable: false),
                        IsSliderFeatured = c.Boolean(nullable: false),
                        ViewCount = c.Long(nullable: false),
                        BookmarkCount = c.Long(nullable: false),
                        AverageRating = c.Decimal(nullable: false, precision: 3, scale: 2),
                        TotalRatings = c.Int(nullable: false),
                        TotalChapters = c.Int(nullable: false),
                        WordCount = c.Long(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsPremium = c.Boolean(nullable: false),
                        ModerationStatus = c.String(maxLength: 20),
                        ModeratedBy = c.Int(),
                        ModerationDate = c.DateTime(),
                        ModerationNotes = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Authors", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.Chapters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NovelId = c.Int(nullable: false),
                        ChapterNumber = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        Content = c.String(nullable: false),
                        WordCount = c.Int(nullable: false),
                        PublishDate = c.DateTime(nullable: false),
                        IsPublished = c.Boolean(nullable: false),
                        UnlockPrice = c.Int(nullable: false),
                        FreePreviewWords = c.Int(nullable: false),
                        IsEarlyAccess = c.Boolean(nullable: false),
                        IsPremium = c.Boolean(nullable: false),
                        ViewCount = c.Long(nullable: false),
                        ModerationStatus = c.String(maxLength: 20),
                        ModeratedBy = c.Int(),
                        ModerationDate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Novels", t => t.NovelId, cascadeDelete: true)
                .Index(t => new { t.NovelId, t.ChapterNumber }, unique: true);
            
            CreateTable(
                "dbo.NovelGenres",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NovelId = c.Int(nullable: false),
                        GenreId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Genres", t => t.GenreId, cascadeDelete: true)
                .ForeignKey("dbo.Novels", t => t.NovelId, cascadeDelete: true)
                .Index(t => new { t.NovelId, t.GenreId }, unique: true);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        IconClass = c.String(maxLength: 50),
                        ColorCode = c.String(maxLength: 7),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NovelGenres", "NovelId", "dbo.Novels");
            DropForeignKey("dbo.NovelGenres", "GenreId", "dbo.Genres");
            DropForeignKey("dbo.Chapters", "NovelId", "dbo.Novels");
            DropForeignKey("dbo.Novels", "AuthorId", "dbo.Authors");
            DropIndex("dbo.NovelGenres", new[] { "NovelId", "GenreId" });
            DropIndex("dbo.Chapters", new[] { "NovelId", "ChapterNumber" });
            DropIndex("dbo.Novels", new[] { "AuthorId" });
            DropTable("dbo.Genres");
            DropTable("dbo.NovelGenres");
            DropTable("dbo.Chapters");
            DropTable("dbo.Novels");
            DropTable("dbo.Authors");
        }
    }
}
