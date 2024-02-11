using Ecommerce_project_1.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Models
{
    public class OrderDetail
    {
        public int Id{ get; set; }
        public int OrderHeaderId{ get; set; }
        public OrderHeader OrderHeader { get; set; }
        public int ProductId{ get; set; }
        public Product Product { get; set; }
        public int Count{ get; set; }
        public double Price{ get; set; }
    }
}
