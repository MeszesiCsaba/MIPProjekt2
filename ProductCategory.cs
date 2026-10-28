using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdministratieMagazine
{
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        // One category can contain many products.
        public virtual ICollection<Product> Products { get; set; }

        public ProductCategory()
        {
            Name = string.Empty;
            Products = new List<Product>();
        }
    }
}