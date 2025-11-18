using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Controllers
{
    public class RankingController : Controller
    {

        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: Ranking
        public ActionResult Index(string type = "view")
        {
            IQueryable<Novel> query = db.Novels
                .Where(n => n.IsActive && n.ModerationStatus == "Approved");

            switch (type.ToLower())
            {
                case "rating":
                    query = query.OrderByDescending(n => n.AverageRating);
                    break;
                case "bookmark":
                    query = query.OrderByDescending(n => n.BookmarkCount);
                    break;
                default:
                    query = query.OrderByDescending(n => n.ViewCount);
                    break;
            }

            var topNovels = query.Take(30).ToList();
            ViewBag.Type = type;

            return View(topNovels);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult Details(int id)
        {
            // id này có thể là NovelId
            return RedirectToAction("BookDetail", "Book", new { id = id });
        }
    }
}
