using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using System.Data.Entity;
using System.IO;

namespace WebNovel.Areas.Admin.Controllers.Novel_ManagerController
{
    public class NovelDetails_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public ActionResult Novel_Details(int? id, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Novel_Manager", "Novel_Manager");
            ViewBag.Genres = new List<Genre>();
            ViewBag.Tags = new List<Tag>();

            if (!id.HasValue || id.Value <= 0)
            {
                TempData["ErrorMessage"] = "Invalid Novel ID provided.";
                return View(CreateEmptyNovel("Invalid ID"));
            }

            try
            {
                var novel = db.Novels.FirstOrDefault(n => n.Id == id.Value);

                if (novel == null)
                {
                    TempData["ErrorMessage"] = $"Novel with ID {id.Value} not found.";
                    return View(CreateEmptyNovel("Novel Not Found"));
                }

                // Increment view count when someone views the novel details
                IncrementViewCount(novel.Id);

                // Load related data separately to avoid Include issues
                LoadNovelRelatedData(novel);

                // Load ViewBag data
                LoadViewBagData();

                return View(novel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General exception in Novel_Details: {ex.Message}");
                TempData["ErrorMessage"] = "Database error: " + ex.Message;
                return View(CreateEmptyNovel("Database Error"));
            }
        }

        private void IncrementViewCount(int novelId)
        {
            try
            {
                var novel = db.Novels.Find(novelId);
                if (novel != null)
                {
                    novel.ViewCount++;
                    novel.UpdatedAt = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error incrementing view count: {ex.Message}");
                // Don't throw exception for view count errors, just log them
            }
        }

        private string GetClientIPAddress()
        {
            try
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0].Trim();
                    }
                }

                return Request.ServerVariables["REMOTE_ADDR"] ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private void LoadNovelRelatedData(Novel novel)
        {
            try
            {
                // Load Author
                if (novel.AuthorId != 0)
                {
                    novel.Author = db.Authors.FirstOrDefault(a => a.Id == novel.AuthorId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Author: {ex.Message}");
            }

            try
            {
                // Load Genres
                var novelGenres = db.NovelGenres.Where(ng => ng.NovelId == novel.Id).ToList();
                novel.NovelGenres = novelGenres;

                foreach (var ng in novelGenres)
                {
                    ng.Genre = db.Genres.FirstOrDefault(g => g.Id == ng.GenreId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Genres: {ex.Message}");
                novel.NovelGenres = new List<NovelGenre>();
            }

            try
            {
                // Load Tags
                var novelTags = db.NovelTags.Where(nt => nt.NovelId == novel.Id).ToList();
                novel.NovelTags = novelTags;

                foreach (var nt in novelTags)
                {
                    nt.Tag = db.Tags.FirstOrDefault(t => t.Id == nt.TagId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Tags: {ex.Message}");
                novel.NovelTags = new List<NovelTag>();
            }

            try
            {
                // Load Chapters
                novel.Chapters = db.Chapters.Where(c => c.NovelId == novel.Id).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Chapters: {ex.Message}");
                novel.Chapters = new List<Chapter>();
            }
        }

        private void LoadViewBagData()
        {
            try
            {
                ViewBag.Genres = db.Genres.Where(g => g.IsActive).OrderBy(g => g.Name).ToList();
                ViewBag.Tags = db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ViewBag data: {ex.Message}");
                ViewBag.Genres = new List<Genre>();
                ViewBag.Tags = new List<Tag>();
            }
        }

        private Novel CreateEmptyNovel(string errorTitle)
        {
            return new Novel
            {
                Id = 0,
                Title = errorTitle,
                Status = "Error",
                ModerationStatus = "Error",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = false,
                IsPremium = false,
                IsOriginal = false,
                IsFeatured = false,
                IsWeeklyFeatured = false,
                IsSliderFeatured = false,
                ViewCount = 0,
                BookmarkCount = 0,
                TotalChapters = 0,
                TotalRatings = 0,
                WordCount = 0,
                AverageRating = 0,
                Chapters = new List<Chapter>(),
                NovelGenres = new List<NovelGenre>(),
                NovelTags = new List<NovelTag>()
            };
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}