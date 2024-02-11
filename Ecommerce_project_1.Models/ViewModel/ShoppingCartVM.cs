using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<Cart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<Address> UserAddresses { get; set; }

    }
}
