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

            // Store original count
            var totalCount = db.Genres.Count();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(g => g.Name.Contains(search) || g.Description.Contains(search));
            }

            // Apply status filter
            switch (statusFilter.ToLower())
            {
                case "active":
                    query = query.Where(g => g.IsActive);
                    break;
                case "inactive":
                    query = query.Where(g => !g.IsActive);
                    break;
                case "all":
                default:
                    break;
            }

            // Get filtered count before sorting
            var filteredCount = query.Count();

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "newest":
                    query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.CreatedAt) : query.OrderByDescending(g => g.CreatedAt);
                    break;
                case "oldest":
                    query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.CreatedAt) : query.OrderByDescending(g => g.CreatedAt);
                    break;
                case "name_asc":
                case "name":
                    query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name);
                    break;
                case "id_asc":
                case "id":
                    query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.Id) : query.OrderByDescending(g => g.Id);
                    break;
                default:
                    query = sortDirection.ToLower() == "asc" ? query.OrderBy(g => g.CreatedAt) : query.OrderByDescending(g => g.CreatedAt);
                    break;
            }

            var totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);

            var genres = query.Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToList();

            // Check if filters are active
            bool hasActiveFilters = !string.IsNullOrEmpty(search) || statusFilter != "all";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.FilteredCount = filteredCount;
            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter; // Changed from Filter to StatusFilter
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.HasActiveFilters = hasActiveFilters;

            return View(genres);
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