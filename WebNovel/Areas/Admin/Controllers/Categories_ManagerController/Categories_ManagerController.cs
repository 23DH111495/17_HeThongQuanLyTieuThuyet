using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using System.Data.Entity;

namespace WebNovel.Areas.Admin.Controllers.Categories_ManagerController
{
    public class Categories_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public ActionResult Categories_Manager(string search = "", string statusFilter = "all", string sortBy = "newest", string sortDirection = "desc", int page = 1, int pageSize = 10)
        {
            var query = db.Genres.AsQueryable();

            // Status filter
            switch (statusFilter.ToLower())
            {
                case "active":
                    query = query.Where(g => g.IsActive);
                    break;
                case "inactive":
                    query = query.Where(g => !g.IsActive);
                    break;
            }

            List<Genre> genres;
            int filteredCount;

            if (!string.IsNullOrEmpty(search))
            {
                // Kiểm tra có tên nào khớp chính xác không
                int parsedId;
                Genre exactMatch = null;

                // Nếu người dùng nhập số → thử tìm theo ID trước
                if (int.TryParse(search, out parsedId))
                {
                    exactMatch = query.FirstOrDefault(g => g.Id == parsedId);
                }

                // Nếu chưa tìm thấy → thử so khớp chính xác theo tên
                if (exactMatch == null)
                {
                    exactMatch = query.FirstOrDefault(g => g.Name.Equals(search, StringComparison.OrdinalIgnoreCase));
                }

                if (exactMatch != null)
                {
                    // Nếu có tên trùng khớp hoàn toàn → chỉ hiện 1 kết quả đó
                    genres = new List<Genre> { exactMatch };
                    filteredCount = 1;
                }
                else
                {
                    // Nếu không có trùng hoàn toàn → tìm gần giống như cũ
                    var filteredList = query
                        .Where(g => g.Name.Contains(search) || g.Description.Contains(search))
                        .ToList();

                    var orderedList = filteredList
                        .OrderByDescending(g => GetRelevanceScore(g, search))
                        .ThenBy(g => g.Name)
                        .ToList();

                    filteredCount = orderedList.Count;
                    genres = orderedList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            else
            {
                // Sort bình thường nếu không search
                switch (sortBy.ToLower())
                {
                    case "name":
                    case "name_asc":
                    case "name_desc":
                        query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name);
                        break;

                    case "id":
                    case "id_asc":
                    case "id_desc":
                        query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.Id) : query.OrderByDescending(g => g.Id);
                        break;

                    case "created":
                    case "newest":
                    case "oldest":
                    default:
                        query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.CreatedAt) : query.OrderByDescending(g => g.CreatedAt);
                        break;
                }

                filteredCount = query.Count();
                genres = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = db.Genres.Count();
            ViewBag.FilteredCount = filteredCount;
            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) || statusFilter != "all";

            return View(genres);
        }
        [HttpGet]
        public JsonResult GetGenresSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new string[] { }, JsonRequestBehavior.AllowGet);

            // Lấy những thể loại đang active và chứa từ khóa
            var suggestions = db.Genres
                .Where(g => g.IsActive && g.Name.Contains(term))
                .Select(g => g.Name)
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }
        private int GetRelevanceScore(Genre g, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return 0;

            keyword = keyword.ToLower();
            string name = g.Name?.ToLower() ?? "";
            string desc = g.Description?.ToLower() ?? "";

            // 1. Name trùng cả cụm
            if (name.Contains(keyword))
                return 5;

            // 2. Name không trùng cả cụm → so chữ cái đầu
            if (!string.IsNullOrEmpty(name) && name[0] == keyword[0])
                return 4;

            // 3. Description trùng cả cụm
            if (!string.IsNullOrEmpty(desc) && desc.Contains(keyword))
                return 3;

            // 4. Description không trùng cả cụm → so chữ cái đầu
            if (!string.IsNullOrEmpty(desc) && desc[0] == keyword[0])
                return 2;

            // 5. Không trùng
            return 1;
        }


        public ActionResult Create(string search = "", string statusFilter = "all", string sortBy = "newest", string sortDirection = "desc", int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.Page = page;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Genre model, string search = "", string statusFilter = "all", string sortBy = "newest", string sortDirection = "desc", int page = 1)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check for duplicate names
                    if (db.Genres.Any(g => g.Name.ToLower() == model.Name.ToLower()))
                    {
                        ModelState.AddModelError("Name", "A genre with this name already exists.");
                        ViewBag.Search = search;
                        ViewBag.StatusFilter = statusFilter;
                        ViewBag.SortBy = sortBy;
                        ViewBag.SortDirection = sortDirection;
                        ViewBag.Page = page;
                        return View(model);
                    }

                    // Additional validation for length constraints
                    if (model.Name.Length > 50)
                    {
                        ModelState.AddModelError("Name", "Name cannot exceed 50 characters.");
                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500) // Adjust max length as needed
                    {
                        ModelState.AddModelError("Description", "Description cannot exceed 500 characters.");
                        return View(model);
                    }

                    var genre = new Genre
                    {
                        Name = model.Name.Trim(),
                        Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim(),
                        IconClass = string.IsNullOrEmpty(model.IconClass) ? null : model.IconClass.Trim(),
                        ColorCode = string.IsNullOrEmpty(model.ColorCode) ? "#77dd77" : model.ColorCode.Trim(),
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.Now,
                        CreatedBy = null  
                    };

                    // Add explicit validation before saving
                    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(genre);
                    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(genre, validationContext, validationResults, true);

                    if (!isValid)
                    {
                        foreach (var validationResult in validationResults)
                        {
                            ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? "", validationResult.ErrorMessage);
                        }
                        return View(model);
                    }

                    db.Genres.Add(genre);

                    // Use SaveChanges() with better error handling
                    try
                    {
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Genre created successfully!";
                        return RedirectToAction("Categories_Manager", new { search, statusFilter, sortBy, sortDirection, page });
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        // Handle Entity Framework validation errors
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException dbUpdateEx)
                    {
                        // Handle database update errors (foreign key, unique constraints, etc.)
                        var innerException = dbUpdateEx.InnerException?.InnerException?.Message ?? dbUpdateEx.InnerException?.Message ?? dbUpdateEx.Message;
                        ModelState.AddModelError("", "Database error: " + innerException);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException}");

                ModelState.AddModelError("", "Error creating genre: " + ex.Message);
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", "Inner exception: " + ex.InnerException.Message);

                    // If there's a deeper inner exception, show that too
                    if (ex.InnerException.InnerException != null)
                    {
                        ModelState.AddModelError("", "Deeper exception: " + ex.InnerException.InnerException.Message);
                    }
                }
            }

            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.Page = page;
            return View(model);
        }

        public ActionResult Edit(int id, string search = "", string statusFilter = "all", string sortBy = "newest", string sortDirection = "desc", int page = 1)
        {
            var genre = db.Genres.Find(id);
            if (genre == null)
            {
                TempData["ErrorMessage"] = "Genre not found.";
                return RedirectToAction("Categories_Manager", new { search, statusFilter, sortBy, sortDirection, page });
            }

            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.Page = page;
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Genre model, string search = "", string statusFilter = "all", string sortBy = "newest", string sortDirection = "desc", int page = 1)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingGenre = db.Genres.Find(model.Id);
                    if (existingGenre == null)
                    {
                        TempData["ErrorMessage"] = "Genre not found.";
                        return RedirectToAction("Categories_Manager", new { search, statusFilter, sortBy, sortDirection, page });
                    }

                    if (db.Genres.Any(g => g.Name.ToLower() == model.Name.ToLower() && g.Id != model.Id))
                    {
                        ModelState.AddModelError("Name", "A genre with this name already exists.");
                        ViewBag.Search = search;
                        ViewBag.StatusFilter = statusFilter;
                        ViewBag.SortBy = sortBy;
                        ViewBag.SortDirection = sortDirection;
                        ViewBag.Page = page;
                        return View(model);
                    }

                    existingGenre.Name = model.Name.Trim();
                    existingGenre.Description = model.Description?.Trim();
                    existingGenre.IconClass = model.IconClass?.Trim();
                    existingGenre.ColorCode = model.ColorCode?.Trim() ?? "#77dd77";
                    existingGenre.IsActive = model.IsActive;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Genre updated successfully!";
                    return RedirectToAction("Categories_Manager", new { search, statusFilter, sortBy, sortDirection, page });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating genre: " + ex.Message);
            }

            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.Page = page;
            return View(model);
        }
        public ActionResult Delete(int id, string search = "", string filter = "all", string sortBy = "newest", int page = 1)
        {
            var genre = db.Genres.Find(id);
            if (genre == null)
            {
                TempData["ErrorMessage"] = "Genre not found.";
                return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
            }

            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.SortBy = sortBy;
            ViewBag.Page = page;
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string search = "", string filter = "all", string sortBy = "newest", int page = 1)
        {
            try
            {
                var genre = db.Genres.Find(id);
                if (genre == null)
                {
                    TempData["ErrorMessage"] = "Genre not found.";
                    return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
                }

                var isUsed = db.NovelGenres.Any(ng => ng.GenreId == id);
                if (isUsed)
                {
                    TempData["ErrorMessage"] = "Cannot delete this genre because it is being used by one or more novels.";
                    return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
                }

                db.Genres.Remove(genre);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Genre deleted successfully!";
                return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting genre: " + ex.Message;
                return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, string search = "", string filter = "all", string sortBy = "newest", int page = 1)
        {
            try
            {
                var genre = db.Genres.Find(id);
                if (genre == null)
                {
                    TempData["ErrorMessage"] = "Genre not found.";
                    return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
                }

                genre.IsActive = !genre.IsActive;
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Genre {(genre.IsActive ? "activated" : "deactivated")} successfully!";
                return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating status: " + ex.Message;
                return RedirectToAction("Categories_Manager", new { search, filter, sortBy, page });
            }
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