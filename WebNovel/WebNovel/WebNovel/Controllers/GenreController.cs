using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.GenreViewModels;
using WebNovel.Models.ViewModels;
using System.Data.Entity; // cần cho Include

namespace WebNovel.Controllers
{
    public class GenresController : Controller
    {
        private readonly DarkNovelDbContext db = new DarkNovelDbContext();

        [HttpGet]
        public ActionResult UserGenres(string genres = "", int page = 1, int pageSize = 10)
        {
            // Lấy tất cả thể loại
            var allGenres = db.Genres.AsNoTracking().ToList();
            ViewBag.AllGenres = allGenres;

            // Các thể loại được chọn
            var selectedGenres = string.IsNullOrEmpty(genres)
                ? new List<string>()
                : genres.Split(',').Select(g => g.Trim()).ToList();
            ViewBag.SelectedGenres = selectedGenres;

            // Lấy tất cả truyện đang hoạt động, chỉ include Genre
            var novels = db.Novels
                .Include(n => n.NovelGenres.Select(ng => ng.Genre))
                .Include(n => n.NovelTags.Select(nt => nt.Tag))
                .AsNoTracking()
                .Where(n => n.IsActive)
                .ToList(); // đưa xuống memory để xử lý MatchCount

            // Lấy danh sách Author tương ứng
            var authorIds = novels.Select(n => n.AuthorId).Distinct().ToList();
            var authors = db.Authors
                .Include(a => a.User)
                .Where(a => authorIds.Contains(a.Id))
                .ToList();

            // Map Author vào truyện và tính MatchCount
            var novelsWithAuthor = novels
                .Select(n =>
                {
                    var matchCount = selectedGenres.Count(g => n.NovelGenres.Any(ng => ng.Genre.Name == g));

                    // Lấy hashtags của truyện
                    var hashtags = n.NovelTags.Select(nt => nt.Tag.DisplayName).ToList();


                    var author = authors.FirstOrDefault(a => a.Id == n.AuthorId);

                    return new GenreListViewModel
                    {
                        Id = n.Id,
                        Title = n.Title,
                        CoverImageUrl = n.CoverImageUrl,
                        CoverImage = n.CoverImage,
                        CoverImageContentType = n.CoverImageContentType,
                        Genres = n.NovelGenres.Select(ng => ng.Genre.Name).ToList(),
                        Hashtags = hashtags,
                        MatchCount = matchCount,
                        Author = author == null ? null : new AuthorViewModel
                        {
                            Id = author.Id,
                            PenName = author.PenName,
                            Biography = author.Biography,
                            FullName = author.User != null
                                ? (!string.IsNullOrEmpty(author.User.FirstName)
                                    ? author.User.FirstName + " " + author.User.LastName
                                    : author.User.Username)
                                : author.PenName
                        }
                    };
                })
                .Where(x => !selectedGenres.Any() || x.MatchCount > 0) // nếu có tag, chỉ lấy truyện match
                .OrderByDescending(x => x.MatchCount) // match nhiều lên đầu
                .ThenBy(x => x.Title)
                .ToList();

            // Phân trang
            var totalNovels = novelsWithAuthor.Count;
            var pagedNovels = novelsWithAuthor.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalNovels / pageSize);
            ViewBag.GenresQuery = string.Join(",", selectedGenres);

            return View(pagedNovels);
        }

        [HttpPost]
        public ActionResult SearchByGenres(List<string> genres)
        {
            if (genres == null || genres.Count == 0)
                return RedirectToAction("UserGenres");

            string genresQuery = string.Join(",", genres);
            return RedirectToAction("UserGenres", new { genres = genresQuery, page = 1 });
        }
    }
}
