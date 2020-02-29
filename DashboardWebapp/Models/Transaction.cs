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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Transaction()
        {
            TransactionTags = new HashSet<Transaction_Tag>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public double Amount { get; set; }

        public DateTime Date { get; set; }

        [StringLength(50)]
        public string Company { get; set; }

        public int? RecurringTransactionId { get; set; }

        public int? TrackerId { get; set; }

        public int PersonId { get; set; }

        public virtual Person Person { get; set; }

        public virtual RecurringTransaction RecurringTransaction { get; set; }

        public virtual Tracker Tracker { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_Tag> TransactionTags { get; set; }
    }
}
