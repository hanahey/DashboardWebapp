namespace DashboardWebapp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Transaction_Tag
    {
        public int Id { get; set; }

        public int TransactionId { get; set; }

        public int TagId { get; set; }

        public virtual Tag Tag { get; set; }

        public virtual Transaction Transaction { get; set; }
    }
}
