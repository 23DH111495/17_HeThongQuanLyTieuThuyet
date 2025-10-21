using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;

namespace WebNovel.Areas.Admin.Controllers
{
    public class CoinPackageController : Controller
    {
        private readonly DarkNovelDbContext db;

        public CoinPackageController()
        {
            db = new DarkNovelDbContext();
        }

        // Dependency injection constructor (optional - for testability)
        public CoinPackageController(DarkNovelDbContext context)
        {
            db = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Admin/CoinPackage - List all packages
        public ActionResult Index()
        {
            try
            {
                var packages = db.CoinPackages
                    .OrderBy(p => p.SortOrder)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToList();

                return View(packages);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading packages: " + ex.Message;
                return View(new List<CoinPackage>());
            }
        }

        // GET: Admin/CoinPackage/Details/5 - View package details
        public ActionResult Details_CoinPackage(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid package ID.";
                return RedirectToAction("Index");
            }

            try
            {
                var package = db.CoinPackages.Find(id);
                if (package == null)
                {
                    TempData["ErrorMessage"] = "Package not found.";
                    return RedirectToAction("Index");
                }

                // Calculate additional stats for display using CoinPurchaseHistory
                var packageStats = db.PurchaseHistories
                    .Where(p => p.PackageId == id && p.PaymentStatus == "Completed")
                    .GroupBy(p => 1)
                    .Select(g => new {
                        TotalSales = g.Count(),
                        TotalRevenue = g.Sum(p => p.PricePaid)
                    })
                    .FirstOrDefault();

                ViewBag.TotalSales = packageStats?.TotalSales ?? 0;
                ViewBag.TotalRevenue = packageStats?.TotalRevenue ?? 0;

                return View(package);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading package details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/CoinPackage/Create - Show create form
        public ActionResult Create_CoinPackage()
        {
            var model = new CoinPackage
            {
                IsActive = true,
                SortOrder = GetNextSortOrder(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return View(model);
        }

        // POST: Admin/CoinPackage/Create - Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_CoinPackage(CoinPackage model)
        {
            try
            {
                // Debug: Log model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    TempData["ErrorMessage"] = "Validation errors: " + string.Join("; ",
                        errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
                    return View(model);
                }

                // Validate business rules
                if (!ValidatePackageData(model))
                {
                    return View(model);
                }

                // Check for duplicate package names
                if (db.CoinPackages.Any(p => p.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Name", "A package with this name already exists.");
                    return View(model);
                }

                // Set timestamps
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                // Calculate VND price if needed (configurable exchange rate)
                if (!model.PriceVND.HasValue || model.PriceVND <= 0)
                {
                    model.PriceVND = CalculateVNDPrice(model.PriceUSD);
                }

                // Set sort order if not provided
                if (model.SortOrder <= 0)
                {
                    model.SortOrder = GetNextSortOrder();
                }

                // Debug: Log before save
                System.Diagnostics.Debug.WriteLine($"Attempting to save package: {model.Name}, Price: {model.PriceUSD}, Coins: {model.CoinAmount}");

                db.CoinPackages.Add(model);
                int result = db.SaveChanges();

                // Debug: Log save result
                System.Diagnostics.Debug.WriteLine($"Save result: {result} rows affected");

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Package created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to save package. No rows affected.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating package: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");

                // Debug: Log full exception
                System.Diagnostics.Debug.WriteLine($"Exception creating package: {ex}");

                return View(model);
            }
        }

        // GET: Admin/CoinPackage/Edit/5 - Show edit form
        public ActionResult Edit_CoinPackage(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid package ID.";
                return RedirectToAction("Index");
            }

            try
            {
                var package = db.CoinPackages.Find(id);
                if (package == null)
                {
                    TempData["ErrorMessage"] = "Package not found.";
                    return RedirectToAction("Index");
                }

                return View(package);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading package for editing: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/CoinPackage/Edit/5 - Handle edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_CoinPackage(int id, CoinPackage model)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid package ID.";
                return RedirectToAction("Index");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    TempData["ErrorMessage"] = "Validation errors: " + string.Join("; ",
                        errors.SelectMany(e => e.Errors.Select(err => $"{e.Field}: {err}")));
                    return View(model);
                }

                // Validate business rules
                if (!ValidatePackageData(model))
                {
                    return View(model);
                }

                var existingPackage = db.CoinPackages.Find(id);
                if (existingPackage == null)
                {
                    TempData["ErrorMessage"] = "Package not found.";
                    return RedirectToAction("Index");
                }

                // Check for duplicate names (excluding current package)
                if (db.CoinPackages.Any(p =>
                    p.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) &&
                    p.Id != id))
                {
                    ModelState.AddModelError("Name", "A package with this name already exists.");
                    return View(model);
                }

                // Update properties
                existingPackage.Name = model.Name;
                existingPackage.CoinAmount = model.CoinAmount;
                existingPackage.BonusCoins = model.BonusCoins;
                existingPackage.PriceUSD = model.PriceUSD;
                existingPackage.IsActive = model.IsActive;
                existingPackage.IsFeatured = model.IsFeatured;
                existingPackage.SortOrder = model.SortOrder;
                existingPackage.UpdatedAt = DateTime.Now;

                // Update VND price
                if (!model.PriceVND.HasValue || model.PriceVND <= 0)
                {
                    existingPackage.PriceVND = CalculateVNDPrice(model.PriceUSD);
                }
                else
                {
                    existingPackage.PriceVND = model.PriceVND;
                }

                int result = db.SaveChanges();

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Package updated successfully!";
                    return RedirectToAction("Details_CoinPackage", new { id = id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update package. No changes detected.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating package: " + ex.Message +
                    (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
                return View(model);
            }
        }

        // GET: Admin/CoinPackage/Delete/5 - Show delete confirmation
        public ActionResult Delete_CoinPackage(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid package ID.";
                return RedirectToAction("Index");
            }

            try
            {
                var package = db.CoinPackages.Find(id);
                if (package == null)
                {
                    TempData["ErrorMessage"] = "Package not found.";
                    return RedirectToAction("Index");
                }

                // Get sales statistics for display using CoinPurchaseHistory
                var packageStats = db.PurchaseHistories
                    .Where(p => p.PackageId == id && p.PaymentStatus == "Completed")
                    .GroupBy(p => 1)
                    .Select(g => new {
                        TotalSales = g.Count(),
                        TotalRevenue = g.Sum(p => p.PricePaid)
                    })
                    .FirstOrDefault();

                ViewBag.TotalSales = packageStats?.TotalSales ?? 0;
                ViewBag.TotalRevenue = packageStats?.TotalRevenue ?? 0;
                ViewBag.HasPurchases = (packageStats?.TotalSales ?? 0) > 0;

                return View(package);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading package for deletion: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/CoinPackage/Delete/5 - Handle delete confirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid package ID.";
                return RedirectToAction("Index");
            }

            try
            {
                var package = db.CoinPackages.Find(id);
                if (package == null)
                {
                    TempData["ErrorMessage"] = "Package not found.";
                    return RedirectToAction("Index");
                }

                // Check if package has been purchased using CoinPurchaseHistory
                var hasPurchases = db.PurchaseHistories.Any(p => p.PackageId == id);

                if (hasPurchases)
                {
                    // Deactivate instead of delete for packages with purchases
                    package.IsActive = false;
                    package.UpdatedAt = DateTime.Now;
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Package has been deactivated successfully (cannot delete due to existing purchases).";
                }
                else
                {
                    // Hard delete for packages without purchases
                    db.CoinPackages.Remove(package);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Package deleted successfully!";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting package: " + ex.Message;
                return RedirectToAction("Delete_CoinPackage", new { id = id });
            }
        }

        // Toggle package status (Active/Inactive)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid package ID." });
            }

            try
            {
                var package = db.CoinPackages.Find(id);
                if (package == null)
                {
                    return Json(new { success = false, message = "Package not found." });
                }

                package.IsActive = !package.IsActive;
                package.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                string status = package.IsActive ? "activated" : "deactivated";

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Package has been {status} successfully!",
                        isActive = package.IsActive
                    });
                }

                TempData["SuccessMessage"] = $"Package has been {status} successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error updating package status: " + ex.Message });
                }

                TempData["ErrorMessage"] = "Error updating package status: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // AJAX endpoint for checking duplicate names
        [HttpPost]
        public JsonResult CheckDuplicateName(string name, int? id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { isDuplicate = false });
                }

                bool isDuplicate;
                if (id.HasValue)
                {
                    // Editing - exclude current record
                    isDuplicate = db.CoinPackages.Any(p =>
                        p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        p.Id != id.Value);
                }
                else
                {
                    // Creating new
                    isDuplicate = db.CoinPackages.Any(p =>
                        p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                return Json(new { isDuplicate = isDuplicate });
            }
            catch (Exception ex)
            {
                // Log exception if you have logging
                return Json(new { isDuplicate = false, error = ex.Message });
            }
        }

        // Bulk operations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkToggleStatus(int[] packageIds, bool activate)
        {
            try
            {
                if (packageIds == null || packageIds.Length == 0)
                {
                    TempData["ErrorMessage"] = "No packages selected.";
                    return RedirectToAction("Index");
                }

                var packages = db.CoinPackages
                    .Where(p => packageIds.Contains(p.Id))
                    .ToList();

                foreach (var package in packages)
                {
                    package.IsActive = activate;
                    package.UpdatedAt = DateTime.Now;
                }

                db.SaveChanges();

                string action = activate ? "activated" : "deactivated";
                TempData["SuccessMessage"] = $"{packages.Count} package(s) have been {action} successfully!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating packages: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #region Helper Methods

        // Helper method to get next sort order
        private int GetNextSortOrder()
        {
            try
            {
                var maxSortOrder = db.CoinPackages.Max(p => (int?)p.SortOrder) ?? 0;
                return maxSortOrder + 1;
            }
            catch (Exception)
            {
                // If table is empty or error occurs, return 1
                return 1;
            }
        }

        // Helper method to calculate VND price
        private decimal CalculateVNDPrice(decimal usdPrice)
        {
            // You might want to get this from configuration or database
            const decimal exchangeRate = 24000m;
            return Math.Round(usdPrice * exchangeRate, 0);
        }

        // Helper method to validate package data
        private bool ValidatePackageData(CoinPackage package)
        {
            bool isValid = true;

            if (package.CoinAmount <= 0)
            {
                ModelState.AddModelError("CoinAmount", "Coin amount must be greater than 0.");
                isValid = false;
            }

            if (package.PriceUSD <= 0)
            {
                ModelState.AddModelError("PriceUSD", "Price must be greater than 0.");
                isValid = false;
            }

            if (package.BonusCoins < 0)
            {
                ModelState.AddModelError("BonusCoins", "Bonus coins cannot be negative.");
                isValid = false;
            }

            if (package.SortOrder < 0)
            {
                ModelState.AddModelError("SortOrder", "Sort order cannot be negative.");
                isValid = false;
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(package.Name))
            {
                ModelState.AddModelError("Name", "Package name is required.");
                isValid = false;
            }

            return isValid;
        }

        #endregion

        // Dispose database context
        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}