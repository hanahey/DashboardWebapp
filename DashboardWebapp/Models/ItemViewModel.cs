using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebapp.Models
{
    public class ItemViewModel
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

        public int PersonId { get; set; }

        public virtual Person Person { get; set; }

        [Display(Name = "Tags")]
        public IEnumerable<Tag> TagCollection { get; set; }

        [Display(Name = "Tags")]
        public IEnumerable<int> TagIds { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        public virtual ICollection<Item_Tag> Item_Tag { get; set; }
    }
}