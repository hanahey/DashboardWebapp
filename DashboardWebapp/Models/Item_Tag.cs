namespace DashboardWebapp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Item_Tag
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public int TagId { get; set; }

        public virtual Item Item { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
