using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string ApplicationUserID { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public DateTime OrderDate{ get; set; }
        [Required]
        public DateTime ShippingDate{ get; set; }
        [Required]
        public double OrderTotal { get; set; }
        public string TrackingNumber{ get; set; }
        public string Carrier{ get; set; }
        public string OrderStatus{ get; set; }
        public string PaymentStatus{ get; set; }
        public DateTime PaymentDate{ get; set; }
        public DateTime PaymentDueDate{ get; set; }
        public string TransactionId{ get; set; }
        [Required]
        public string Name{ get; set; }
        [Required]
        [Display(Name = "Street Address")]
        public string StreetAddress{ get; set; }
        [Required]
        public string City{ get; set; }
        [Required]
        public string State{ get; set; }
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode{ get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber{ get; set; }

    }
}
