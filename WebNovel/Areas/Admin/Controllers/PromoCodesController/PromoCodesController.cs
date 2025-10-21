using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Areas.Admin.Controllers
{
    public class PromoCodesController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();

        // GET: Admin/PromoCodes
        public ActionResult Index_PromoCodes(string search = "", string sortBy = "created",
            string sortDirection = "desc", string statusFilter = "all", string activeFilter = "all",
            string promoTypeFilter = "all", int page = 1)
        {
            var query = db.PromoCodes
                .Include(p => p.Creator)
                .Include(p => p.UsageHistory)
                .AsQueryable();

            var totalCount = query.Count();

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Code.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    p.PromoType.ToLower().Contains(search)
                );
            }

            // Status Filter (Active/Inactive)
            if (activeFilter != "all")
            {
                bool isActive = activeFilter == "active";
                query = query.Where(p => p.IsActive == isActive);
            }

            // Promo Type Filter
            if (promoTypeFilter != "all")
            {
                query = query.Where(p => p.PromoType == promoTypeFilter);
            }

            // Status Filter (Expired/Valid)
            DateTime now = DateTime.Now;
            if (statusFilter != "all")
            {
                switch (statusFilter)
                {
                    case "expired":
                        query = query.Where(p => p.ValidUntil.HasValue && p.ValidUntil.Value < now);
                        break;
                    case "valid":
                        query = query.Where(p => !p.ValidUntil.HasValue || p.ValidUntil.Value >= now);
                        break;
                    case "maxed":
                        query = query.Where(p => p.MaxUses.HasValue && p.UsedCount >= p.MaxUses.Value);
                        break;
                    case "available":
                        query = query.Where(p => !p.MaxUses.HasValue || p.UsedCount < p.MaxUses.Value);
                        break;
                }
            }

            var filteredCount = query.Count();

            // Sorting
            switch (sortBy.ToLower())
            {
                case "code":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.Code) :
                        query.OrderByDescending(p => p.Code);
                    break;
                case "type":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.PromoType) :
                        query.OrderByDescending(p => p.PromoType);
                    break;
                case "value":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.Value) :
                        query.OrderByDescending(p => p.Value);
                    break;
                case "used":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.UsedCount) :
                        query.OrderByDescending(p => p.UsedCount);
                    break;
                case "expiry":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.ValidUntil ?? DateTime.MaxValue) :
                        query.OrderByDescending(p => p.ValidUntil ?? DateTime.MaxValue);
                    break;
                case "status":
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.IsActive) :
                        query.OrderByDescending(p => p.IsActive);
                    break;
                default: // "created"
                    query = sortDirection == "asc" ?
                        query.OrderBy(p => p.CreatedAt) :
                        query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var promoCodes = query.ToList();

            // Set ViewBag properties for maintaining filter state
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.ActiveFilter = activeFilter;
            ViewBag.PromoTypeFilter = promoTypeFilter;
            ViewBag.TotalCount = totalCount;
            ViewBag.FilteredCount = filteredCount;
            ViewBag.HasActiveFilters = !string.IsNullOrEmpty(search) ||
                                     statusFilter != "all" ||
                                     activeFilter != "all" ||
                                     promoTypeFilter != "all";

            return View(promoCodes);
        }

        // GET: Admin/PromoCodes/Details/5
        public ActionResult Details_PromoCodes(int id)
        {
            var promoCode = db.PromoCodes
                .Include(p => p.Creator)
                .Include(p => p.UsageHistory.Select(u => u.User))
                .FirstOrDefault(p => p.Id == id);

            if (promoCode == null)
            {
                TempData["ErrorMessage"] = "Promo code not found.";
                return RedirectToAction("Index_PromoCodes");
            }

            return View(promoCode);
        }

        // GET: Admin/PromoCodes/Create
        public ActionResult Create_PromoCodes()
        {
            var model = new PromoCode
            {
                ValidFrom = DateTime.Now,
                ValidUntil = DateTime.Now.AddDays(30),
                IsActive = true
            };

            ViewBag.PromoTypes = GetPromoTypeSelectList();
            return View(model);
        }

        // POST: Admin/PromoCodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_PromoCodes(PromoCode model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if code already exists
                    if (db.PromoCodes.Any(p => p.Code == model.Code))
                    {
                        ModelState.AddModelError("Code", "This promo code already exists.");
                        ViewBag.PromoTypes = GetPromoTypeSelectList();
                        return View(model);
                    }

                    // Set creation info (you might want to get this from session/identity)
                    model.CreatedBy = GetCurrentUserId(); // Implement this method
                    model.CreatedAt = DateTime.Now;
                    model.UsedCount = 0;

                    db.PromoCodes.Add(model);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Promo code created successfully!";
                    return RedirectToAction("Index_PromoCodes");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating promo code: " + ex.Message;
            }

            ViewBag.PromoTypes = GetPromoTypeSelectList();
            return View(model);
        }

        // GET: Admin/PromoCodes/Edit/5
        public ActionResult Edit_PromoCodes(int id)
        {
            var promoCode = db.PromoCodes.Find(id);
            if (promoCode == null)
            {
                TempData["ErrorMessage"] = "Promo code not found.";
                return RedirectToAction("Index_PromoCodes");
            }

            ViewBag.PromoTypes = GetPromoTypeSelectList(promoCode.PromoType);
            return View(promoCode);
        }

        // POST: Admin/PromoCodes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_PromoCodes(int id, PromoCode model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingPromoCode = db.PromoCodes.Find(id);
                    if (existingPromoCode == null)
                    {
                        TempData["ErrorMessage"] = "Promo code not found.";
                        return RedirectToAction("Index_PromoCodes");
                    }

                    // Check if code already exists (excluding current record)
                    if (db.PromoCodes.Any(p => p.Code == model.Code && p.Id != id))
                    {
                        ModelState.AddModelError("Code", "This promo code already exists.");
                        ViewBag.PromoTypes = GetPromoTypeSelectList(model.PromoType);
                        return View(model);
                    }

                    // Update fields
                    existingPromoCode.Code = model.Code;
                    existingPromoCode.Description = model.Description;
                    existingPromoCode.PromoType = model.PromoType;
                    existingPromoCode.Value = model.Value;
                    existingPromoCode.MaxUses = model.MaxUses;
                    existingPromoCode.ValidFrom = model.ValidFrom;
                    existingPromoCode.ValidUntil = model.ValidUntil;
                    existingPromoCode.IsActive = model.IsActive;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Promo code updated successfully!";
                    return RedirectToAction("Index_PromoCodes");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating promo code: " + ex.Message;
            }

            ViewBag.PromoTypes = GetPromoTypeSelectList(model.PromoType);
            return View(model);
        }

        // GET: Admin/PromoCodes/Delete/5
        public ActionResult Delete_PromoCodes(int id)
        {
            var promoCode = db.PromoCodes
                .Include(p => p.Creator)
                .Include(p => p.UsageHistory)
                .FirstOrDefault(p => p.Id == id);

            if (promoCode == null)
            {
                TempData["ErrorMessage"] = "Promo code not found.";
                return RedirectToAction("Index_PromoCodes");
            }

            return View(promoCode);
        }

        // POST: Admin/PromoCodes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            try
            {
                var promoCode = db.PromoCodes.Find(id);
                if (promoCode == null)
                {
                    TempData["ErrorMessage"] = "Promo code not found.";
                    return RedirectToAction("Index_PromoCodes");
                }

                // Check if promo code has been used
                if (promoCode.UsedCount > 0)
                {
                    TempData["ErrorMessage"] = "Cannot delete a promo code that has been used. Consider deactivating it instead.";
                    return RedirectToAction("Details_PromoCodes", new { id = id });
                }

                db.PromoCodes.Remove(promoCode);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Promo code deleted successfully!";
                return RedirectToAction("Index_PromoCodes");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting promo code: " + ex.Message;
                return RedirectToAction("Details_PromoCodes", new { id = id });
            }
        }

        // AJAX: Toggle Active Status
        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            try
            {
                var promoCode = db.PromoCodes.Find(id);
                if (promoCode == null)
                {
                    return Json(new { success = false, message = "Promo code not found." });
                }

                promoCode.IsActive = !promoCode.IsActive;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Promo code {(promoCode.IsActive ? "activated" : "deactivated")} successfully!",
                    isActive = promoCode.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Helper methods
        private SelectList GetPromoTypeSelectList(string selectedValue = null)
        {
            var promoTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Free Coins", Value = "FreeCoins" },
                new SelectListItem { Text = "Discount Percentage", Value = "DiscountPercent" },
                new SelectListItem { Text = "Fixed Discount", Value = "DiscountFixed" }
            };

            return new SelectList(promoTypes, "Value", "Text", selectedValue);
        }

        private int? GetCurrentUserId()
        {
            // Implement this based on your authentication system
            // For example, if using Identity:
            // return User.Identity.GetUserId<int>();

            // For now, returning null - replace with actual implementation
            return null;
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