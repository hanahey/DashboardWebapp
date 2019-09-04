using DashboardWebapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DashboardWebapp.Controllers
{
    public class CategoriesController : Controller
    {
        DashboardContext db = new DashboardContext();

        // GET: Categories
        public ActionResult Index()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

        // GET: Categories/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Categories/Create
        public ActionResult AddCategory()
        {
            return PartialView();
        }

        // POST: Categories/Create
        [HttpPost]
        public ActionResult AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category { Name = model.Name };
                db.Categories.Add(category);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return PartialView();
            }
        }

        // GET: Categories/Edit/5
        public ActionResult EditCategory(int id)
        {
            var category = db.Categories.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        public ActionResult EditCategory(int id, Category category)
        {
            var thisCategory = db.Categories.Where(t => t.Id == id).FirstOrDefault();
            thisCategory.Name = category.Name;

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["Success"] = "Changes Saved!";
                return RedirectToAction("Index");
            }
            else
            {
                return PartialView();
            }
        }

        // GET: Categories/Delete/5
        public ActionResult DeleteCategory(int id)
        {
            var category = db.Categories.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        public ActionResult DeleteCategory(int id, Category category)
        {
            try
            {
                var thisCategory = (from c in db.Categories where c.Id == id select c).First();
                db.Categories.Remove(thisCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return PartialView();
            }
        }
    }
}
