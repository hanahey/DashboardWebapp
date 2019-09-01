using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebapp.Models
{
    public class TransactionViewModel
    {

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public double Amount { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Category")]
        public Category Category { get; set; }

        public virtual RecurringTransaction RecurringTransaction { get; set; }

        [Display(Name = "Period")]
        public virtual Period Period { get; set; }

        [Display(Name = "Tracker")]
        public Tracker Tracker { get; set; }
    }
}