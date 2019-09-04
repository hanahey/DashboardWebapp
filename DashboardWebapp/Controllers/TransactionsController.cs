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
        DashboardContext db = new DashboardContext();
        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = from t in db.Transactions
                               select new TransactionViewModel
                               {
                                   Id = t.Id,
                                   Name = t.Name,
                                   Date = t.Date,
                                   Amount = t.Amount,
                                   Category = t.Category,
                                   Tracker = t.Tracker,
                                   RecurringTransaction = t.RecurringTransaction,
                               };

            // retrieve Period --> from recurring transaction
            return View(transactions);
        }

        // GET: Transactions/Create
        public ActionResult AddTransaction()
        {
            TransactionViewModel transaction = new TransactionViewModel();
            transaction.CategoryCollection = db.Categories.ToList<Category>();
            transaction.PeriodCollection = db.Periods.ToList<Period>();
            transaction.TrackerCollection = db.Trackers.ToList<Tracker>();

            return PartialView(transaction);
        }

        // POST: Transactions/Create
        [HttpPost]
        public ActionResult AddTransaction(TransactionViewModel model)
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            int currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

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
                        CategoryId = model.CategoryId,
                        TrackerId = model.TrackerId,
                        PersonId = currentPersonId,
                    };
                    db.Transactions.Add(transaction);
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
                        CategoryId = model.CategoryId,
                        TrackerId = model.TrackerId,
                        PersonId = currentPersonId,
                        RecurringTransactionId = recurringTransaction.Id
                    };
                    db.Transactions.Add(transaction);
                }                
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return PartialView();
            }
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int id)
        {
            return PartialView();
        }

        // POST: Transactions/Edit/5
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
                return PartialView();
            }
        }

        // GET: Transactions/Delete/5
        public ActionResult DeleteTransaction(int id)
        {
            var transaction = (from t in db.Transactions
                               where t.Id == id select t).First();
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
                if (thisTransaction.RecurringTransaction !=null)
                {
                    var recurringTransaction = (from t in db.RecurringTransactions where t.Id == thisTransaction.RecurringTransactionId
                                                select t).First();
                    recurringTransaction.EndDate = DateTime.Now;
                }

                db.Transactions.Remove(thisTransaction); //remove AFTER checking for RecurringTransaction or else RecurringTransaction will always be null
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
