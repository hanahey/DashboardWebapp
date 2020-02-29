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
    public class TagsController : Controller
    {
        DataContext db = new DataContext();
        static int currentPersonId;

        // GET: Tags
        public ActionResult Index()
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;
            var tags = from t in db.Tags where t.PersonId == currentPersonId select t;
            return View(tags);
        }

        // GET: Tags/AddTag
        public ActionResult AddTag()
        {
            return PartialView();
        }

        // POST: Tags/AddTag
        [HttpPost]
        public ActionResult AddTag(Tag model)
        {
            if (ModelState.IsValid)
            {
                var tag = new Tag {Name = model.Name, PersonId = currentPersonId };
                db.Tags.Add(tag);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return PartialView(model);
            }
        }

        // GET: Tags/EditTag/5
        public ActionResult EditTag(int id)
        {
            var tag = db.Tags.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(tag);
        }

        // POST: Tags/EditTag/5
        [HttpPost]
        public ActionResult EditTag(int id, Tag tag)
        {
            var thisTag = db.Tags.Where(t => t.Id == id).FirstOrDefault();
            thisTag.Name = tag.Name;

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["Success"] = "Changes Saved!";
                return RedirectToAction("Index");
            }
            else
            {
                return PartialView(tag);
            }
        }

        // GET: Tags/DeleteTag/5
        public ActionResult DeleteTag(int id)
        {
            var tag = db.Tags.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost]
        public ActionResult DeleteTag(int id, Tag tag)
        {
            try
            {
                var thisTag = (from t in db.Tags where t.Id == id select t).First();
                db.Tags.Remove(thisTag);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return PartialView(tag);
            }
        }
    }
}
