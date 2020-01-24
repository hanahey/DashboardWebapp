using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DashboardWebapp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DashboardWebapp.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
       DashboardContext db = new DashboardContext();
        static int currentPersonId;
        // GET: Items
        public ActionResult Index()
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                 GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;
            var items = from i in db.Items where i.PersonId == currentPersonId
                         orderby i.Id descending select i;
            return View(items);
        }

        // GET: Items/AddItem
        public ActionResult AddItem()
        {
            Item item = new Item();
            item.CategoryCollection = db.Categories.ToList<Category>();

            return PartialView(item);
        }

        // POST: Items/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItem(Item model)
        {
            if (ModelState.IsValid)
            {
                var item = new Item
                {
                    Name = model.Name,
                    Price = model.Price,
                    Store = model.Store,
                    Quantity = model.Quantity,
                    Measurement = model.Measurement,
                    CategoryId = model.CategoryId,
                    PersonId = currentPersonId,
                };
                db.Items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.CategoryCollection = db.Categories.ToList<Category>();
                return PartialView(model);
            }
        }

        // GET: Items/EditItem/5
        public ActionResult EditItem(int id)
        {
            var item = db.Items.Where(i => i.Id == id).FirstOrDefault();

            //Populate category dropdown list
            item.CategoryCollection = db.Categories.ToList<Category>();

            return PartialView(item);
        }

        // POST: Items/EditItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem(int id, Item model)
        {
            var item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            item.Name = model.Name;
            item.Price = model.Price;
            item.Store = model.Store;
            item.Quantity = model.Quantity;
            item.Measurement = model.Measurement;
            item.CategoryId = model.CategoryId;

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["Success"] = "Changes Saved!";
                return RedirectToAction("Index");
            }
            else
            {
                model.CategoryCollection = db.Categories.ToList<Category>();
                return PartialView(model);
            }
        }

        // GET: Items/Delete/5
        public ActionResult DeleteItem(int id)
        {
            var item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            return PartialView(item);
        }

        // POST: Items/Delete/5
        [HttpPost]
        public ActionResult DeleteItem(int id, Item item)
        {
            item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
