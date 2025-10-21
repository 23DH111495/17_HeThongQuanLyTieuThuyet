using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.ViewModels;

namespace WebNovel.Areas.Admin.Controllers.Coin_ManagerController
{
    public class Coin_ManagerController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();
        // GET: Admin/Coin_Manager
        public ActionResult Coin_Manager()
        {
            var activePackages = db.CoinPackages
                                    .Where(c => c.IsActive)
                                    .OrderBy(c => c.SortOrder)
                                    .Take(3)
                                    .ToList();
            var activePromo=db.PromoCodes
                                    .Where(p => p.IsActive )
                                    .OrderBy(p => p.CreatedAt)
                                    .Take(3)
                                    .ToList();
            var viewModel = new CoinManagerViewModel
            {
                ActivePackages = activePackages,
                ActivePromos = activePromo
            };
            return View("Coin_Manager", viewModel);
        }

        // GET: Admin/Coin_Manager/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Admin/Coin_Manager/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Coin_Manager/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Coin_Manager/Edit/5
        public ActionResult Edit(int id)
        {
            var package = db.CoinPackages.FirstOrDefault(x => x.Id == id);
            if (package == null)
                return HttpNotFound();

            return View(package);
        }

        // POST: Admin/Coin_Manager/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Coin_Manager/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Admin/Coin_Manager/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
