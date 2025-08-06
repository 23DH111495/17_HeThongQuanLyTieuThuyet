using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebNovel
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "BookDetail",
                url: "Book/BookDetail/{id}",
                defaults: new { controller = "Book", action = "BookDetail" },
                constraints: new { id = @"\d+" }
            );

            // Move comment system routes BEFORE BookActions to avoid conflicts
            routes.MapRoute(
                name: "AddComment",
                url: "Book/AddComment",
                defaults: new { controller = "Book", action = "AddComment" }
            );

            routes.MapRoute(
                name: "EditComment",
                url: "Book/EditComment",
                defaults: new { controller = "Book", action = "EditComment" }
            );

            routes.MapRoute(
                name: "DeleteComment",
                url: "Book/DeleteComment",
                defaults: new { controller = "Book", action = "DeleteComment" }
            );

            routes.MapRoute(
                name: "LikeComment",
                url: "Book/LikeComment",
                defaults: new { controller = "Book", action = "LikeComment" }
            );

            routes.MapRoute(
                name: "LoadMoreComments",
                url: "Book/LoadMoreComments",
                defaults: new { controller = "Book", action = "LoadMoreComments" }
            );

            // Update BookActions to include comment system actions OR remove the constraint
            routes.MapRoute(
                name: "BookActions",
                url: "Book/{action}",
                defaults: new { controller = "Book" },
                constraints: new { action = @"^(DebugAuth|DebugComments|DiagnoseImages|GetCoverImage|GetUserAvatar|AddComment|EditComment|DeleteComment|LikeComment|LoadMoreComments)$" }
            );

            routes.MapRoute(
                name: "BookBySlug",
                url: "Book/{slug}",
                defaults: new { controller = "Book", action = "BookDetailBySlug" }
            );

            routes.MapRoute(
                name: "ChapterReadBySlug",
                url: "read/{novelSlug}/chapter-{chapterNumber}",
                defaults: new { controller = "ChapterReader", action = "ReadBySlug" },
                constraints: new { chapterNumber = @"\d+" }
            );

            routes.MapRoute(
                name: "ChapterReadByIdShort",
                url: "Chapter/Read/{id}",
                defaults: new { controller = "ChapterReader", action = "Read" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "ChapterReadById",
                url: "ChapterReader/Read/{id}",
                defaults: new { controller = "ChapterReader", action = "Read" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "ViewChapterByNumber",
                url: "Admin/Chapter_Manager/ViewChapter/{novelId}/{chapterNumber}",
                defaults: new { area = "Admin", controller = "Chapter_Manager", action = "ViewChapterByNumber" },
                constraints: new { novelId = @"\d+", chapterNumber = @"\d+" },
                namespaces: new[] { "WebNovel.Areas.Admin.Controllers" }
            );

            routes.MapRoute(
                name: "ChapterDetail",
                url: "Admin/Chapter_Manager/DetailChapter/{novelId}",
                defaults: new { area = "Admin", controller = "Chapter_Manager", action = "DetailChapter", novelId = UrlParameter.Optional },
                namespaces: new[] { "WebNovel.Areas.Admin.Controllers" }
            );

            routes.MapRoute(
                name: "BookDetailBySlug",
                url: "book/{slug}",
                defaults: new { controller = "Book", action = "BookDetailBySlug" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}