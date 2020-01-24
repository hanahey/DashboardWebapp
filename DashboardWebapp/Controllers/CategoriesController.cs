using DashboardWebapp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DashboardWebapp.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        DashboardContext db = new DashboardContext();
        static int currentPersonId;

        // GET: Categories
        public ActionResult Index()
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;
            var categories = from c in db.Categories where c.PersonId == currentPersonId select c;
            return View(categories);
        }

        // GET: Categories/AddCategory
        public ActionResult AddCategory()
        {
            return PartialView();
        }

        // POST: Categories/AddCategory
        [HttpPost]
        public ActionResult AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category { Name = model.Name, PersonId = currentPersonId };
                db.Categories.Add(category);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return PartialView(model);
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
                return PartialView(category);
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
                return PartialView(category);
            }
        }
    }
}
