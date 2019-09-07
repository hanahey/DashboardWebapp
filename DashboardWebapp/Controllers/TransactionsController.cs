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
    public class TransactionsController : Controller
    {
        DashboardContext db = new DashboardContext();
        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = from t in db.Transactions orderby t.Id descending 
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
                                                    CategoryId = t.CategoryId,
                                                    PeriodId = t.RecurringTransaction.PeriodId,
                                                    StartDate = t.RecurringTransaction.StartDate,
                                                    EndDate = t.RecurringTransaction.EndDate,
                                                    RecurringTransactionId = t.RecurringTransactionId,
                                                    TrackerId = t.TrackerId,
                                                }).First();
            if (transaction.Amount < 0)
            {
                transaction.Amount = -transaction.Amount;
                transaction.Direction = "Out";
            }
            else
                transaction.Direction = "In";

            //Populate dropdown lists
            transaction.CategoryCollection = db.Categories.ToList<Category>();
            transaction.PeriodCollection = db.Periods.ToList<Period>();
            transaction.TrackerCollection = db.Trackers.ToList<Tracker>();

            ViewBag.Date = transaction.Date.ToString("yyyy-MM-dd"); //to pre-populate datepicker

            //store StartDate in ViewBag to compare with Date + to pre-populate datepicker if StartDate != Date
            ViewBag.StartDate = null; // set to null for condition check in View
            if (transaction.StartDate != null)
            {
                ViewBag.StartDate = ((DateTime)(transaction.StartDate)).ToString("yyyy-MM-dd");  //to pre-populate EndDate datepicker
            }
            //store EndDate in ViewBag to pre-populate datepicker
            ViewBag.EndDate = null; // set to null for condition check in View
            if (transaction.EndDate!=null)
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
            var recurringTransaction = (from t in db.RecurringTransactions where t.Id == transaction.RecurringTransactionId select t).First();
            var firstTransaction = (from x in recurringTransaction.Transactions orderby x.Date select x).First();

             //set the Start Date as the Date for a recurring transaction without any other transactions
            if (transaction.RecurringTransaction.PeriodId > 0 && ((transaction.RecurringTransaction.Transactions.Count == 1) ||
               ((transaction.RecurringTransaction.Transactions.Count > 1) && (transaction == firstTransaction))))
            {
                transaction.Date = (DateTime)model.StartDate;
            }
            else
                transaction.Date = model.Date;

            if (model.Direction == "Out") 
                model.Amount = -(model.Amount);
            transaction.Name = model.Name;
            transaction.Amount = model.Amount; 
            
            transaction.TrackerId = model.TrackerId;
            transaction.CategoryId = model.CategoryId;
            // transaction.RecurringTransactionId = model.RecurringTransactionId; -- read only

            if (transaction.RecurringTransactionId != null)
            {                
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
