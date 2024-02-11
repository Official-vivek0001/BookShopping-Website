using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Utility
{
    public class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Individual = "Individual User";
        public const string Role_Company = "Company User";
        public const string Role_Employee = "Employee User";

        //Session Cart Count
        public const string CartSessionCount = "Cart Count Session ";

        //order Status
        public const string OrderStatus_Processing= "Processing";
        public const string OrderStatus_Pending = "Pending";
        public const string OrderStatus_Approved = "Approved";
        public const string OrderStatus_Shipped = "Shipped";
        public const string OrderStatus_Cancelled = "Cancelled";
        public const string OrderStatus_Refund = "Refund";

        //PaYment Status
        public const string PaymentStatus_Pending = "Pending";
        public const string PaymentStatus_Approved = "Approved";
        public const string PaymentStatus_Delayed = "PaymentStatusDelayed";
        public const string PaymentStatus_Rejected = "Rejected";

        public const string Ss_CartSessionCount = "Cart Count Session";



        public static double GetPriceBasedOnQuantity(double quantity,double price,double price50,double price100)
        {
            if (quantity >50)
            {
                return price;
            }
            else if (quantity < 100)
            {
                return price50;

            }
            else
            {
                return price100;
            }

        }
    }

}
