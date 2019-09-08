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
    public class HomeController : Controller
    {
        DashboardContext db = new DashboardContext();
        public ActionResult Index()
        {
            return View();
        }
        
        //Tracker methods:
        public ActionResult Trackers()
        {
            //currently show only ongoing trackers     
            var ongoingTrackers = (from t in db.Trackers
                                  where t.EndDate == null || t.EndDate >= DateTime.Now
                                  select new TrackerViewModel
                                  {
                                      Id = t.Id,
                                      Name = t.Name,
                                      GoalAmount = t.GoalAmount,
                                      StartDate = t.StartDate,
                                      GoalDate = t.GoalDate,
                                      EndDate = t.EndDate,
                                      PersonId = t.PersonId,
                                      RecurringTransaction = (from rt in db.RecurringTransactions
                                                              where rt.Id == ((from trans in db.Transactions
                                                                               where trans.RecurringTransactionId != null && trans.TrackerId == t.Id
                                                                               && (trans.RecurringTransaction.EndDate == null ||
                                                                               trans.RecurringTransaction.EndDate > DateTime.Now)
                                                                               select trans.RecurringTransactionId).FirstOrDefault())
                                                              select rt).FirstOrDefault(),
                                      Transactions = t.Transactions,
                                  }).ToList();

            //need to manually change all negative Amount values to positive to calculate amount in tracker so far
            foreach (TrackerViewModel t in ongoingTrackers)
            {
                double amountSaved = 0;
                foreach (Transaction trans in t.Transactions)
                {
                    if (trans.Amount < 0)
                        trans.Amount = -(trans.Amount);
                    amountSaved += trans.Amount;

                }
                ongoingTrackers.Where(x => x.Id == t.Id).First().AmountSaved = amountSaved;
            }

            //TO-DO LATER: Completed trackers
            var completedTrackers = from t in db.Trackers where t.EndDate != null && t.EndDate < DateTime.Now.Date select t;
            ViewBag.CompletedTrackers = completedTrackers;

            return PartialView(ongoingTrackers);
        }

        public ActionResult OneTimePayment(int id)
        {
            var tracker = (from t in db.Trackers where t.Id == id select t).First(); //populate tracker field
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.CategoryCollection = db.Categories.ToList<Category>();
            transaction.Tracker = tracker;
            return PartialView(transaction);


        }

        [HttpPost]
        public ActionResult OneTimePayment(int id, Transaction transaction)
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            int currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                var newOneTimePayment = new Transaction
                {
                    Name = transaction.Name,
                    Amount = -(transaction.Amount),
                    Date = transaction.Date,
                    TrackerId = id,
                    CategoryId = transaction.CategoryId,
                    PersonId = currentPersonId
                };
                db.Transactions.Add(newOneTimePayment);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            else
                return PartialView(transaction);
        }

        public ActionResult AddRecurringPayment(int id)
        {
            var tracker = (from t in db.Trackers where t.Id == id select t).First(); //populate tracker field
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.CategoryCollection = db.Categories.ToList<Category>();
            transaction.PeriodCollection = db.Periods.ToList<Period>();
            transaction.Tracker = tracker;
            return PartialView(transaction);
        }

        [HttpPost]
        public ActionResult AddRecurringPayment(int id, TransactionViewModel model)
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            int currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                var recurringTransaction = new RecurringTransaction
                {
                    Name = model.Name,
                    StartDate = (DateTime)model.StartDate,
                    EndDate = model.EndDate,
                    PeriodId = (int)model.PeriodId,
                };
                db.RecurringTransactions.Add(recurringTransaction);

                var transaction = new Transaction
                {
                    Name = model.Name,
                    Amount = -(model.Amount),
                    Date = (DateTime)model.StartDate,
                    CategoryId = model.CategoryId,
                    TrackerId = id,
                    PersonId = currentPersonId,
                    RecurringTransactionId = recurringTransaction.Id
                };
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return PartialView(model);
            }
        }        

        public ActionResult StopRecurringPayment(int id)
        {
            var transaction = (from t in db.Transactions
                               where t.TrackerId == id && t.RecurringTransaction != null orderby t.Id descending
                               select t).First();
            var recurringTransaction = (from rt in db.RecurringTransactions where 
                                        rt.Id == transaction.RecurringTransactionId select rt).First();
            ViewBag.TrackerName = (from t in db.Trackers where t.Id == id select t.Name).First(); //get tracker
            return PartialView(recurringTransaction);
        }
        
        [HttpPost]
        public ActionResult StopRecurringPayment(int id, RecurringTransaction model)
        {
            try
            {
                var transaction = (from t in db.Transactions
                                   where t.TrackerId == id && t.RecurringTransaction != null
                                   orderby t.Id descending
                                   select t).First();
                var recurringTransaction = (from rt in db.RecurringTransactions
                                            where rt.Id == transaction.RecurringTransactionId
                                            select rt).First();
                recurringTransaction.EndDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return PartialView(model);
            }
        }

        public ActionResult AddTracker()
        {
            return PartialView();
        }
        
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
                    EndDate = model.EndDate,
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
        public ActionResult EditTracker(int id)
        {
            var tracker = db.Trackers.Where(t => t.Id == id).FirstOrDefault();

            //Store EndDate in ViewBag to populate datepicker if EndDate exists in db
            ViewBag.EndDate = null;
            if (tracker.EndDate != null)
            {
                ViewBag.EndDate = ((DateTime)(tracker.EndDate)).ToString("yyyy-MM-dd");
            }
            return PartialView(tracker);
        }
        
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
        
        public ActionResult DeleteTracker(int id)
        {
            var tracker = db.Trackers.Where(t => t.Id == id).FirstOrDefault();
            return PartialView(tracker);
        }
        
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

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}