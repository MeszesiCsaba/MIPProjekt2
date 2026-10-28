using System;
using System.ComponentModel.DataAnnotations;

namespace AdministratieMagazine
{
    public class SalesHistory
    {
        public int Id { get; set; }

        // The product sold in this transaction.
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }

        // The number of units sold.
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // When the sale happened.
        public DateTime SoldAt { get; set; }

        // Navigation property to the related product.
        public virtual Product Product { get; set; }

        public SalesHistory()
        {
            SoldAt = DateTime.Now;
            ProductName = string.Empty;
            CategoryName = string.Empty;
        }
    }
}