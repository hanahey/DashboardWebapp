﻿using DashboardWebapp.Models;
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
        DataContext db = new DataContext();
        static int currentPersonId;

        public ActionResult Index()
        {
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
            GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;
            return View();
        }

        //Cash flow methods:
        public ActionResult CashFlow()
        {
            var thisMonth = DateTime.Now.Month;
            var transactions = (from t in db.Transactions
                               where t.Date.Month == thisMonth
                               && t.PersonId == currentPersonId
                               orderby t.Date descending
                               select new TransactionViewModel
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                                   Date = t.Date,
                                   Amount = t.Amount,
                                   Company = t.Company,
                                   Tracker = t.Tracker,
                                   RecurringTransaction = t.RecurringTransaction,
                               }).ToList();
        
            //value for 'IN'
            var cashFlowIn = from t in transactions where t.Amount > 0 select t;
            ViewBag.CashFlowIn = 0;
            double cashFlowInTotal = 0;

            foreach (TransactionViewModel t in cashFlowIn)
            {
                double thisinAmount = 0;
                if (t != null)
                {
                    thisinAmount = t.Amount;
                }

                cashFlowInTotal += thisinAmount;
            }

            ViewBag.CashFlowIn = cashFlowInTotal;


            //value for 'OUT'
            var cashFlowOut = from t in transactions where t.Amount < 0 select t;
            double cashFlowOutTotal = 0;
            foreach (TransactionViewModel t in cashFlowOut)
            {
                double thisOutAmount = -t.Amount;
                cashFlowOutTotal += thisOutAmount;
            }
            ViewBag.CashFlowOut = cashFlowOutTotal;

            //value for trackers
            var cashFlowTrackers = from t in transactions where t.Tracker != null select t;
            double cashFlowTrackersTotal = 0;
            foreach (TransactionViewModel t in cashFlowTrackers)
            {
                double thisTrackerAmount = t.Amount;
                cashFlowTrackersTotal += thisTrackerAmount;
            }
            ViewBag.CashFlowTrackers = cashFlowTrackersTotal;

            //value for expenses
            var cashFlowExpenses = from t in transactions where t.Amount < 0 && t.Tracker == null select t;
            double cashFlowExpensesTotal = 0;
            foreach (TransactionViewModel t in cashFlowExpenses)
            {
                double thisCashFlowExpense = -t.Amount;
                cashFlowExpensesTotal += thisCashFlowExpense;
            }
            ViewBag.CashFlowExpenses = cashFlowExpensesTotal;

            //get all tags mapped to all of this user's transactions
            foreach (TransactionViewModel transaction in transactions)
            {
                var thisTransTags = (from t in db.Transactions
                                     join transTag in db.TransactionTags on t.Id equals transTag.TransactionId
                                     join tag in db.Tags on transTag.TagId equals tag.Id
                                     where t.Id == transaction.Id
                                     select new
                                     {
                                         Id = tag.Id,
                                         Name = tag.Name,
                                         PersonId = tag.PersonId,
                                     }).ToList();

                transaction.Tags = thisTransTags.Select(x => new Tag
                {
                    Id = x.Id,
                    Name = x.Name,
                    PersonId = x.PersonId,
                }).ToList();
            }

            return PartialView(transactions);
        }

        public ActionResult AddTransaction()
        {
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
            transaction.PeriodCollection = db.Periods.ToList();

            //get TrackerCollection WITHOUT Trackers with a current RecurringTransaction:
            List<Tracker> TrackerCollectionList = (from t in db.Trackers where t.PersonId == currentPersonId select t).ToList();
            //NOTE: take from TRANSACTION db because that is the table linked to the Tracker table
            var recurringTransactionwithTracker = (from t in db.Transactions
                                                   where t.RecurringTransaction != null
                                                   && (t.RecurringTransaction.EndDate == null ||
                                                   t.RecurringTransaction.EndDate > DateTime.Now)
                                                   select t).ToList();

            foreach (var t in recurringTransactionwithTracker)
            {
                if (TrackerCollectionList.Contains(t.Tracker))
                    TrackerCollectionList.Remove(t.Tracker);
            }
            transaction.TrackerCollection = TrackerCollectionList;

            return PartialView(transaction);
        }

        // POST: Transactions/Create
        [HttpPost]
        public ActionResult AddTransaction(TransactionViewModel model)
        {
            Period selectedPeriod = null; //set selected period to null to avoid null reference error when checking period id
            if (model.PeriodId > 0)
            {
                selectedPeriod = (from p in db.Periods where p.Id == model.PeriodId select p).First();
            }

            if (model.Direction == "Out") //append negative symbol to Amount if money is going out
            {
                model.Amount = -model.Amount;
            }

            if (ModelState.IsValid)
            {
                if (selectedPeriod == null)
                {
                    var transaction = new Transaction
                    {
                        Name = model.Name,
                        Amount = model.Amount,
                        Date = model.Date,
                        Company = model.Company,
                        TrackerId = model.TrackerId,
                        PersonId = currentPersonId,
                    };
                    db.Transactions.Add(transaction);

                    if (model.TagIds != null)
                    {
                        foreach (int t in model.TagIds)
                        {
                            var tag = new Transaction_Tag
                            {
                                TransactionId = transaction.Id,
                                TagId = t,
                            };
                            db.TransactionTags.Add(tag);
                        }
                    }
                }
                else //add RecurringTransactionId + RecurringTransaction record if needed
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
                        Amount = model.Amount,
                        Date = (DateTime)model.StartDate,
                        Company = model.Company,
                        TrackerId = model.TrackerId,
                        PersonId = currentPersonId,
                        RecurringTransactionId = recurringTransaction.Id
                    };
                    db.Transactions.Add(transaction);

                    if (model.TagIds != null)
                    {
                        foreach (int t in model.TagIds)
                        {
                            var tag = new Transaction_Tag
                            {
                                TransactionId = transaction.Id,
                                TagId = t,
                            };
                            db.TransactionTags.Add(tag);
                        }
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
                model.PeriodCollection = db.Periods.ToList<Period>();
                List<Tracker> TrackerCollectionList = (from t in db.Trackers where t.PersonId == currentPersonId select t).ToList();
                var recurringTransactionwithTracker = (from t in db.Transactions
                                                       where t.RecurringTransaction != null
                                                       && (t.RecurringTransaction.EndDate == null ||
                                                       t.RecurringTransaction.EndDate > DateTime.Now)
                                                       select t).ToList();

                foreach (var t in recurringTransactionwithTracker)
                {
                    if (TrackerCollectionList.Contains(t.Tracker))
                        TrackerCollectionList.Remove(t.Tracker);
                }
                model.TrackerCollection = TrackerCollectionList;
                return PartialView(model);
            }
        }

        //Tracker methods:
        public ActionResult Trackers()
        {
            //currently show only ongoing trackers     
            var ongoingTrackers = (from t in db.Trackers
                                   where (t.EndDate == null || t.EndDate >= DateTime.Now) && t.PersonId == currentPersonId
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
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
            transaction.Tracker = (from t in db.Trackers where t.Id == id select t).First();
            return PartialView(transaction);
        }

        [HttpPost]
        public ActionResult OneTimePayment(int id, TransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    Name = model.Name,
                    Amount = model.Amount,
                    Date = model.Date,
                    TrackerId = id,
                    PersonId = currentPersonId
                };
                db.Transactions.Add(transaction);

                if (model.TagIds != null)
                {
                    foreach (int t in model.TagIds)
                    {
                        var tag = new Transaction_Tag
                        {
                            TransactionId = transaction.Id,
                            TagId = t,
                        };
                        db.TransactionTags.Add(tag);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");

            }
            else
            {
                model.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
                model.Tracker = (from t in db.Trackers where t.Id == id select t).First();
                return PartialView(model);
            }
        }

        public ActionResult AddRecurringPayment(int id)
        {
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
            transaction.PeriodCollection = db.Periods.ToList<Period>();
            transaction.Tracker = (from t in db.Trackers where t.Id == id select t).First();
            return PartialView(transaction);
        }

        [HttpPost]
        public ActionResult AddRecurringPayment(int id, TransactionViewModel model)
        {
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
                    Amount = model.Amount,
                    Date = (DateTime)model.StartDate,
                    TrackerId = id,
                    PersonId = currentPersonId,
                    RecurringTransactionId = recurringTransaction.Id
                };
                db.Transactions.Add(transaction);

                if (model.TagIds != null)
                {
                    foreach (int t in model.TagIds)
                    {
                        var tag = new Transaction_Tag
                        {
                            TransactionId = transaction.Id,
                            TagId = t,
                        };
                        db.TransactionTags.Add(tag);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
                model.PeriodCollection = db.Periods.ToList<Period>();
                model.Tracker = (from t in db.Trackers where t.Id == id select t).First();
                return PartialView(model);
            }
        }

        public ActionResult StopRecurringPayment(int id)
        {
            var transaction = (from t in db.Transactions
                               where t.TrackerId == id && t.RecurringTransaction != null
                               orderby t.Id descending
                               select t).First();
            var recurringTransaction = (from rt in db.RecurringTransactions
                                        where
    rt.Id == transaction.RecurringTransactionId
                                        select rt).First();
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
            //get logged in user
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
                return PartialView(tracker);
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