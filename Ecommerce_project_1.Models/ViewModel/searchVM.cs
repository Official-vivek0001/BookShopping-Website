using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Models.ViewModel
{
    public class searchVM
    {
      
        public Product Product { get; set;}
        public IEnumerable<Product> SearchProducts { get; set;}
        public List<Product> RealtedProducts { get; set;}
    }
}
