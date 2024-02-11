using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        [Range(1,1000)]
        public double ListPrice { get; set; }
        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }
        [Required]
        [Range(1, 1000)]
        public double Price50 { get; set; }
        [Required]
        [Range(1, 1000)]
        public double Price100 { get; set; }
        [Display(Name ="ImageUrl")]
        public string ImageUrl { get; set; }
        [Display(Name = "Category")]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
        [Display(Name = "Cover Type")]
        public string CoverTypeId { get; set; }
        public CoverType CoverType { get; set; }
        public int SalesCount { get; set; }
       

      
    }
}
