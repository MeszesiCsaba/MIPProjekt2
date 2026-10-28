using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdministratieMagazine
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime DateAdded { get; set; }

        // Null means this product has no expiration date.
        public DateTime? ExpirationDate { get; set; }

        public int Quantity { get; set; }

        // Foreign key to the product category.
        public int CategoryId { get; set; }

        public virtual ProductCategory Category { get; set; }

        public virtual ICollection<SalesHistory> SalesHistory { get; set; }

        public Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            DateAdded = DateTime.Now;
            SalesHistory = new List<SalesHistory>();
        }
    }
}