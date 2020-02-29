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
    public class TransactionsController : Controller
    {
        DataContext db = new DataContext();
        static int currentPersonId;

        // GET: Transactions
        public ActionResult Index()
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

            var transactions = (from t in db.Transactions
                                where t.PersonId == currentPersonId
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

            // get all tags mapped to all of this user's transactions
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

            return View(transactions);
        }

        // GET: Transactions/Create
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

        // GET: Transactions/Edit/5
        public ActionResult EditTransaction(int id)
        {
            TransactionViewModel transaction = (from t in db.Transactions
                                                where t.Id == id
                                                select new TransactionViewModel
                                                {
                                                    Id = t.Id,
                                                    Name = t.Name,
                                                    Date = t.Date,
                                                    Amount = t.Amount,
                                                    Company = t.Company,
                                                    PeriodId = t.RecurringTransaction.PeriodId,
                                                    StartDate = t.RecurringTransaction.StartDate,
                                                    EndDate = t.RecurringTransaction.EndDate,
                                                    RecurringTransactionId = t.RecurringTransactionId,
                                                    TrackerId = t.TrackerId,
                                                }).First();

            //Get all TagIds tagged to this transaction
            var tagIds = (from transTag in db.TransactionTags
                          join tag in db.Tags on transTag.TagId equals tag.Id
                          where transTag.TransactionId == transaction.Id
                          select tag.Id).ToList();
            transaction.TagIds = tagIds;

            transaction.PeriodCollection = db.Periods.ToList<Period>();

            if (transaction.Amount < 0)
            {
                transaction.Amount = -(transaction.Amount);
                transaction.Direction = "Out";
            }
            else
                transaction.Direction = "In";

            //Populate dropdown lists
            transaction.TagCollection = (from t in db.Tags where t.PersonId == currentPersonId select t).ToList();
            transaction.PeriodCollection = db.Periods.ToList<Period>();
            transaction.TrackerCollection = (from t in db.Trackers where t.PersonId == currentPersonId select t).ToList();

            //get TrackerCollection WITHOUT Trackers with a current RecurringTransaction:
            //EXCEPT FOR THIS TRACKER [IF ANY] IF IT IS A RECURRING TRANSACTION
            List<Tracker> TrackerCollectionList = (from t in db.Trackers where t.PersonId == currentPersonId select t).ToList();
            //NOTE: take from TRANSACTION db because that is the table linked to the Tracker table
            var recurringTransactionwithTracker = (from t in db.Transactions
                                                   where t.RecurringTransaction != null
                                                   && t.TrackerId != transaction.TrackerId
                                                   && (t.RecurringTransaction.EndDate == null ||
                                                   t.RecurringTransaction.EndDate > DateTime.Now)
                                                   select t).ToList();

            foreach (var t in recurringTransactionwithTracker)
            {
                if (TrackerCollectionList.Contains(t.Tracker))
                    TrackerCollectionList.Remove(t.Tracker);
            }
            transaction.TrackerCollection = TrackerCollectionList;

            ViewBag.Date = transaction.Date.ToString("yyyy-MM-dd"); //to pre-populate datepicker

            //store StartDate in ViewBag to compare with Date + to pre-populate datepicker if StartDate != Date
            ViewBag.StartDate = null; // set to null for condition check in View
            if (transaction.StartDate != null)
            {
                ViewBag.StartDate = ((DateTime)(transaction.StartDate)).ToString("yyyy-MM-dd");  //to pre-populate EndDate datepicker
            }
            //store EndDate in ViewBag to pre-populate datepicker
            ViewBag.EndDate = null; // set to null for condition check in View
            if (transaction.EndDate != null)
            {
                ViewBag.EndDate = ((DateTime)(transaction.EndDate)).ToString("yyyy-MM-dd");  //to pre-populate EndDate datepicker
            }

            return PartialView(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        public ActionResult EditTransaction(int id, TransactionViewModel model)
        {
            var transaction = (from t in db.Transactions where t.Id == id select t).First();
            transaction.Date = model.Date;
            if (transaction.RecurringTransactionId != null)
            {
                var recurringTransaction = (from t in db.RecurringTransactions where t.Id == transaction.RecurringTransactionId select t).First();
                var firstTransaction = (from x in recurringTransaction.Transactions orderby x.Date select x).First();

                //set the Start Date as the Date for a recurring transaction without any other transactions
                if (transaction.RecurringTransaction.PeriodId > 0 && ((transaction.RecurringTransaction.Transactions.Count == 1) ||
                   ((transaction.RecurringTransaction.Transactions.Count > 1) && (transaction == firstTransaction))))
                {
                    if (model.StartDate != null)
                    {
                        transaction.Date = (DateTime)model.StartDate;
                    }
                }
                else
                    transaction.Date = model.Date;

                recurringTransaction.Name = model.Name;
                if (model.StartDate != null)
                    recurringTransaction.StartDate = (DateTime)model.StartDate;
                if (model.EndDate != null)
                    recurringTransaction.EndDate = (DateTime)model.EndDate;

                /*If a RecurringTransaction is edited and it has more than 1 transaction, modify the names of all its 
                  Transactions to follow the latest RecurringTransaction.Name and append a number (ie. "Name" #2)*/
                if (recurringTransaction.Transactions.Count > 1)
                {
                    var transactionList = from t in db.Transactions where t.RecurringTransactionId == recurringTransaction.Id orderby id select t;
                    int numberTransaction = 0;

                    foreach (Transaction rt in transactionList)
                    {
                        numberTransaction += 1;
                        rt.Name = model.Name + " [" + numberTransaction.ToString() + "]";
                    }
                }
            }

            if (model.Direction == "Out") //append negative symbol to Amount if money is going out
                model.Amount = -(model.Amount);
            transaction.Name = model.Name;
            transaction.Amount = model.Amount;
            transaction.Company = model.Company;
            transaction.TrackerId = model.TrackerId;

            //check for existing Transaction_Tags for this transaction to ensure no duplicates are being added
            List<int> thistransExistingTagIds = (from t in db.Transactions
                                                 join existingTransTag in db.TransactionTags on t.Id equals existingTransTag.TransactionId
                                                 join tag in db.Tags on existingTransTag.TagId equals tag.Id
                                                 where t.Id == transaction.Id
                                                 select tag.Id).ToList();
            if (model.TagIds != null)
            {
                foreach (int t in model.TagIds)
                {
                    if (!thistransExistingTagIds.Contains(t))
                    {
                        var tag = new Transaction_Tag
                        {
                            TransactionId = transaction.Id,
                            TagId = t,
                        };
                        db.TransactionTags.Add(tag);
                    }
                }

                //remove tags from transaction if they have been untagged
                foreach (int tagIdToRemove in thistransExistingTagIds)
                {
                    if (!model.TagIds.Contains(tagIdToRemove))
                    {
                        var tagToRemove = (from t in db.TransactionTags where t.TagId == tagIdToRemove && t.TransactionId == model.Id select t).First();
                        db.TransactionTags.Remove(tagToRemove);
                    }
                }
            }

            //remove all existing tags if none are selected
            else if (model.TagIds == null && thistransExistingTagIds != null)
            {
                foreach (int tagIdToRemove in thistransExistingTagIds)
                {
                    var tagToRemove = (from t in db.TransactionTags where t.TagId == tagIdToRemove && t.TransactionId == model.Id select t).First();
                    db.TransactionTags.Remove(tagToRemove);
                }
            }
    

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["Success"] = "Changes Saved!";
                return RedirectToAction("Index");
            }
            else
            {
                model.TagCollection = (from t in db.Tags where t.PersonId == currentPersonId select t).ToList();
                model.PeriodCollection = db.Periods.ToList<Period>();
                List<Tracker> TrackerCollectionList = (from t in db.Trackers where t.PersonId == currentPersonId select t).ToList();
                var recurringTransactionwithTracker = (from t in db.Transactions
                                                       where t.RecurringTransaction != null
                                                       && t.TrackerId != transaction.TrackerId
                                                       && (t.RecurringTransaction.EndDate == null ||
                                                       t.RecurringTransaction.EndDate > DateTime.Now)
                                                       select t).ToList();
                foreach (var t in recurringTransactionwithTracker)
                {
                    if (TrackerCollectionList.Contains(t.Tracker))
                        TrackerCollectionList.Remove(t.Tracker);
                }
                model.TrackerCollection = TrackerCollectionList;

                ViewBag.Date = model.Date.ToString("yyyy-MM-dd");
                if (model.StartDate != null)
                    ViewBag.StartDate = ((DateTime)(model.StartDate)).ToString("yyyy-MM-dd");
                if (model.EndDate != null)
                    ViewBag.EndDate = ((DateTime)(model.EndDate)).ToString("yyyy-MM-dd");
                return PartialView(model);
            }
        }

        // GET: Transactions/Delete/5
        public ActionResult DeleteTransaction(int id)
        {
            var transaction = (from t in db.Transactions
                               where t.Id == id
                               select t).First();
            return PartialView(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost]
        public ActionResult DeleteTransaction(int id, Transaction transaction)
        {
            try
            {
                var thisTransaction = (from t in db.Transactions where t.Id == id select t).First();

                //set recurring transaction end date to current date time to prevent future transactions
                if (thisTransaction.RecurringTransaction != null)
                {
                    var recurringTransaction = (from t in db.RecurringTransactions
                                                where t.Id == thisTransaction.RecurringTransactionId
                                                select t).First();
                    recurringTransaction.EndDate = DateTime.Now;
                }

                db.Transactions.Remove(thisTransaction); //remove AFTER checking for RecurringTransaction or else RecurringTransaction will always be null
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return PartialView(transaction);
            }
        }
    }
}
