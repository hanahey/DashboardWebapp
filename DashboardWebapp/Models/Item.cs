namespace DashboardWebapp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Item")]
    public partial class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Details")]
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Store { get; set; }

        public int Quantity { get; set; }

        [StringLength(50)]
        public string Measurement { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public int PersonId { get; set; }

        public virtual Category Category { get; set; }

        public virtual Person Person { get; set; }

        public IEnumerable<Category> CategoryCollection { get; set; }
    }
}
                                        