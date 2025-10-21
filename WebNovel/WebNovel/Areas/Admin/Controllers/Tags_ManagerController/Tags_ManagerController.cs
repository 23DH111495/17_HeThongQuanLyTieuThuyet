using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Areas.Admin.Controllers
{
    public class Tags_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        public ActionResult Tags_Manager(
            string search = "", 
            string filter = "all", 
            string sortBy = "newest", 
            string sortOrder = "desc", 
            int page = 1, 
            int pageSize = 10)
        {
            try
            {
                var tagsQuery = db.Tags.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    tagsQuery = tagsQuery.Where(t => t.Name.Contains(search));
                }

                if (filter == "active")
                {
                    tagsQuery = tagsQuery.Where(t => t.IsActive == true);
                }
                else if (filter == "inactive")
                {
                    tagsQuery = tagsQuery.Where(t => t.IsActive == false);
                }

                switch (sortBy.ToLower())
                {
                    case "name":
                        tagsQuery = sortOrder == "desc" ? tagsQuery.OrderByDescending(t => t.Name) : tagsQuery.OrderBy(t => t.Name);
                        break;

                    case "newest":
                        tagsQuery = tagsQuery.OrderByDescending(t => t.CreatedAt);
                        break;

                    case "id":
                    default:
                        tagsQuery = sortOrder == "desc" ? tagsQuery.OrderByDescending(t => t.Id) : tagsQuery.OrderBy(t => t.Id);
                        break;
                }

                var totalCount = tagsQuery.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var tags = tagsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Search = search;
                ViewBag.Filter = filter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = totalPages;
                ViewBag.TagCount = totalCount;

                return View(tags);
            }
            catch (System.Data.Entity.Core.EntityCommandExecutionException ex)
            {
                var innerException = ex.InnerException;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                TempData["ErrorMessage"] = "Database error: " + innerException.Message;
                return View(new List<Tag>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "General error: " + ex.Message;
                return View(new List<Tag>());
            }
        }

        public ActionResult Create()
        {
            return View(new Tag { IsActive = true, Color = "#77dd77" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Clean and validate the tag name
                    if (!string.IsNullOrEmpty(tag.Name))
                    {
                        tag.Name = tag.Name.Trim();
                        if (!tag.Name.StartsWith("#"))
                        {
                            tag.Name = "#" + tag.Name;
                        }
                    }

                    // Clean the description - remove any potentially problematic characters
                    if (!string.IsNullOrEmpty(tag.Description))
                    {
                        tag.Description = tag.Description.Trim();
                        // Optional: Limit description length if your database has constraints
                        if (tag.Description.Length > 1000) // Adjust based on your DB column size
                        {
                            tag.Description = tag.Description.Substring(0, 1000);
                        }
                    }
                    else
                    {
                        // Explicitly set to null if empty to avoid issues
                        tag.Description = null;
                    }

                    // Check for duplicate tag names
                    if (db.Tags.Any(t => t.Name == tag.Name))
                    {
                        ModelState.AddModelError("Name", "Tag name already exists");
                        return View(tag);
                    }

                    // Set creation timestamp
                    tag.CreatedAt = DateTime.Now;

                    // Add and save
                    db.Tags.Add(tag);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Tag created successfully";
                    return RedirectToAction("Tags_Manager");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    // Handle database update exceptions specifically
                    var innerException = ex.InnerException;
                    while (innerException?.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }

                    string errorMessage = innerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "Failed to create tag: " + errorMessage);

                    // Log the full error for debugging
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to create tag: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"General Exception: {ex}");
                }
            }
            return View(tag);
        }

        public ActionResult Edit(int id, string search = "", string filter = "all", string sortBy = "id", string sortOrder = "asc")
        {
            try
            {
                var tag = db.Tags.Find(id);
                if (tag == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Search = search;
                ViewBag.Filter = filter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;
                return View(tag);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading tag: " + ex.Message;
                return RedirectToAction("Tags_Manager", new { search = search, filter = filter, sortBy = sortBy, sortOrder = sortOrder });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tag tag, string search = "", string filter = "all", string sortBy = "id", string sortOrder = "asc")
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Clean and validate the tag name
                    if (!string.IsNullOrEmpty(tag.Name))
                    {
                        tag.Name = tag.Name.Trim();
                        if (!tag.Name.StartsWith("#"))
                        {
                            tag.Name = "#" + tag.Name;
                        }
                    }

                    // Clean the description
                    if (!string.IsNullOrEmpty(tag.Description))
                    {
                        tag.Description = tag.Description.Trim();
                        // Optional: Limit description length if your database has constraints
                        if (tag.Description.Length > 1000) // Adjust based on your DB column size
                        {
                            tag.Description = tag.Description.Substring(0, 1000);
                        }
                    }
                    else
                    {
                        tag.Description = null;
                    }

                    // Check for duplicate tag names (excluding current tag)
                    if (db.Tags.Any(t => t.Name == tag.Name && t.Id != tag.Id))
                    {
                        ModelState.AddModelError("Name", "Tag name already exists");
                        ViewBag.Search = search;
                        ViewBag.Filter = filter;
                        ViewBag.SortBy = sortBy;
                        ViewBag.SortOrder = sortOrder;
                        return View(tag);
                    }

                    // Get existing tag and update properties
                    var existingTag = db.Tags.Find(tag.Id);
                    if (existingTag == null)
                    {
                        return HttpNotFound();
                    }

                    existingTag.Name = tag.Name;
                    existingTag.Description = tag.Description;
                    existingTag.Color = tag.Color;
                    existingTag.IsActive = tag.IsActive;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Tag updated successfully";
                    return RedirectToAction("Tags_Manager", new { search = search, filter = filter, sortBy = sortBy, sortOrder = sortOrder });
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    // Handle database update exceptions specifically
                    var innerException = ex.InnerException;
                    while (innerException?.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }

                    string errorMessage = innerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "Failed to update tag: " + errorMessage);

                    // Log the full error for debugging
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to update tag: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"General Exception: {ex}");
                }
            }
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            try
            {
                var tag = db.Tags.Find(id);
                if (tag != null)
                {
                    tag.IsActive = !tag.IsActive;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = $"Tag {(tag.IsActive ? "activated" : "deactivated")} successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Tag not found";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to toggle tag status: " + ex.Message;
            }
            return RedirectToAction("Tags_Manager");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var tag = db.Tags.Find(id);
                if (tag == null)
                {
                    return HttpNotFound();
                }
                return View(tag);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading tag: " + ex.Message;
                return RedirectToAction("Tags_Manager");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var tag = db.Tags.Find(id);
                if (tag == null)
                {
                    TempData["ErrorMessage"] = "Tag not found";
                    return RedirectToAction("Tags_Manager");
                }

                db.Tags.Remove(tag);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tag deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to delete tag: " + ex.Message;
            }

            return RedirectToAction("Tags_Manager");
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