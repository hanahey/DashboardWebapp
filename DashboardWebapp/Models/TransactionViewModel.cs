using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebapp.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Amount (£)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double Amount { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMM yyyy}")]
        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        public string Company { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public IEnumerable<Category> CategoryCollection { get; set; }

        [Display(Name = "Category")]
        public virtual Category Category { get; set; }

        [Display(Name = "In/Out")]
        public string Direction { get; set; }
        
        [Display(Name = "Frequency")]
        public int? PeriodId { get; set; }

        public IEnumerable<Period> PeriodCollection { get; set; }

        [Display(Name = "Period")]
        public virtual Period Period { get; set; }
      

        [Display(Name = "Tracker")]
        public int? TrackerId { get; set; }

        public IEnumerable<Tracker> TrackerCollection { get; set; }

        public virtual Tracker Tracker { get; set; }

        public int? RecurringTransactionId { get; set; }

        public virtual RecurringTransaction RecurringTransaction { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMM yyyy}")]

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
    }
}