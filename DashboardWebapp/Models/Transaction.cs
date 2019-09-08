namespace DashboardWebapp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Transaction")]
    public partial class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double Amount { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime Date { get; set; }

        public int? CategoryId { get; set; }

        public int? RecurringTransactionId { get; set; }

        public int? TrackerId { get; set; }

        public int PersonId { get; set; }

        public virtual Category Category { get; set; }

        public virtual Person Person { get; set; }

        public virtual RecurringTransaction RecurringTransaction { get; set; }

        public virtual Tracker Tracker { get; set; }
    }
}
