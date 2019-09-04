using DashboardWebapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DashboardWebapp.Controllers
{
    [Authorize]
    public class TrackersController : Controller
    {
        DashboardContext db = new DashboardContext();

        public ActionResult Index()
        {
            var trackers = from t in db.Trackers
                             select t;
            return View(trackers);
        }

         // GET: Trackers/Details/5
        public ActionResult TrackerTransactions(int id)
        {
            var transactions = from trans in db.Transactions where trans.TrackerId == id select trans;
            return View(transactions);
        }
        

        // GET: Trackers/Create
        public ActionResult AddTracker()
        {
            return PartialView();
        }

        // POST: Trackers/Create
        [HttpPost]
        public ActionResult AddTracker(Tracker model)
        {
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            int currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                var tracker = new Tracker
                {
                    Name = model.Name,
                    GoalAmount = model.GoalAmount,
                    StartDate = model.StartDate,
                    GoalDate = model.GoalDate,
                    PersonId = currentPersonId,
                };
                db.Trackers.Add(tracker);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return PartialView();
            }
        }

        // GET: Trackers/Edit/5
        public ActionResult EditTracker(int id)
        {
            var tracker = db.Trackers.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(tracker);
        }

        // POST: Trackers/Edit/5
        [HttpPost]
        public ActionResult EditTracker(int id, Tracker tracker)
        {
            var thisTracker = db.Trackers.Where(t => t.Id == id).FirstOrDefault();
            thisTracker.Name = tracker.Name;
            thisTracker.GoalAmount = tracker.GoalAmount;
            thisTracker.StartDate = tracker.StartDate;
            thisTracker.GoalDate = tracker.GoalDate;
            thisTracker.EndDate = tracker.EndDate;

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
        public ActionResult DeleteTracker(int id)
        {
            var tracker = db.Trackers.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(tracker);
        }

        // POST: Trackers/Delete/5
        [HttpPost]
        public ActionResult DeleteTracker(int id, Tracker model)
        {
            try
            {
                var tracker = (from t in db.Trackers where t.Id == id select t).First();
                db.Trackers.Remove(tracker);
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
