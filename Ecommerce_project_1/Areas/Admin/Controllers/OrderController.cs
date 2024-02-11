using Ecommerce_project_1.DataAccess.Data;
using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Models.ViewModel;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Ecommerce_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        public OrderController(ApplicationDbContext context, IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Pending()
        {
            return View();
        }
        public IActionResult SucceededOrder()
        {
            return View();
        }
        public IActionResult Details(int Id)
        {
            var orderDetails = _unitOfWork.OrderDetail
                  .GetAll(filter: od => od.OrderHeaderId == Id, includeProperties: "Product");


            var orderDetailsVM = orderDetails.Select(od => new OrderDetails
            {
                OrderID = od.OrderHeaderId,
                ProductName = od.Product.Title,
                Price = (decimal)od.Price,
                Quantity = od.Count
            }).ToList();

            return View(orderDetailsVM);
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            
            var OrdersList = _unitOfWork.OrderHeader.GetAll(sc=>sc.ApplicationUser.Id==sc.ApplicationUserID);
            var applicationUser = _unitOfWork.ApplicationUser.GetAll();
          
          
            return Json(new { data = OrdersList });
        }
        [HttpGet]
        public IActionResult GetAllPendingOrders()
        {

            var Orders = _unitOfWork.OrderHeader.GetAll(sc => sc.ApplicationUser.Id == sc.ApplicationUserID).Where(x=>x.OrderStatus==SD.OrderStatus_Pending);
            var applicationUser = _unitOfWork.ApplicationUser.GetAll();


            return Json(new { data = Orders });
        }
        public IActionResult GetAllSucceededOrder()
        {

            var Orders = _unitOfWork.OrderHeader.GetAll(sc => sc.ApplicationUser.Id == sc.ApplicationUserID ).Where(x => x.OrderStatus == SD.OrderStatus_Approved);
            var applicationUser = _unitOfWork.ApplicationUser.GetAll();


            return Json(new { data = Orders });
        }
        #endregion
        public IActionResult CancelOrder(int id)
        {


            var delete = _unitOfWork.OrderHeader.FirstOrDefault(x => x.Id == id, includeProperties: "ApplicationUser");
            if (delete == null) return NotFound();
            var orderDetails = _unitOfWork.OrderDetail.GetAll(filter: o => o.OrderHeaderId == id, includeProperties: "OrderHeader,Product");
            // Send SMS notification
            string cancellationCause = "Due to unavailability of stock";

            // Customize the SMS body in the action
            string smsBody = $"Order Cancellation - Rana Shopping App\n\n{GetOrderDetailsText(orderDetails)}\n\nCancellation Cause: " +
                $"{cancellationCause}\n\nWe regret to inform you that your order has been canceled due to unavailability of stock. We apologize for any inconvenience caused by the cancellation.";

            //_twilioService.SendOrderConfirmationSMS(delete.ApplicationUser.PhoneNumber, smsBody);
            _unitOfWork.OrderHeader.Remove(delete);
            SendCancellationEmail(delete.ApplicationUser.Email, delete.Id, orderDetails.ToList());
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        private string GetOrderDetailsText(IEnumerable<OrderDetail> orderDetails)
        {
            StringBuilder orderDetailsText = new StringBuilder();

            // Customize the order details text based on your requirements
            foreach (var orderDetail in orderDetails)
            {
                orderDetailsText.AppendLine($"Product: {orderDetail.Product.Title}, Quantity: {orderDetail.Count}, Price: ${orderDetail.Price}");
            }

            orderDetailsText.AppendLine($"Total Amount: ${orderDetails.First().OrderHeader.OrderTotal}");

            return orderDetailsText.ToString();
        }





        private void SendCancellationEmail(string customerEmail, int orderId, List<OrderDetail> orderDetails)
        {
            try
            {
                string subject = "Order Cancellation Notification";
                StringBuilder messageBuilder = new StringBuilder();

                // Begin HTML content
                messageBuilder.AppendLine("<!DOCTYPE html>");
                messageBuilder.AppendLine("<html lang=\"en\">");
                messageBuilder.AppendLine("<head>");
                messageBuilder.AppendLine("<meta charset=\"UTF-8\">");
                messageBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                messageBuilder.AppendLine("<title>Order Cancellation</title>");
                messageBuilder.AppendLine("</head>");
                messageBuilder.AppendLine("<body>");

                // Email body
                messageBuilder.AppendLine($"<p>Dear {orderDetails.First().OrderHeader.ApplicationUser.Name},</p>");
                messageBuilder.AppendLine("<p>We regret to inform you that your order with ID " + $"{orderId} has been canceled due to unavailability of stock. The details are as follows:</p>");

                messageBuilder.AppendLine("<table border=\"1\">");
                foreach (var orderDetail in orderDetails)
                {
                    messageBuilder.AppendLine("<tr>");
                    messageBuilder.AppendLine($"<td>Product: {orderDetail.Product.Title}</td>");
                    messageBuilder.AppendLine($"<td>Quantity: {orderDetail.Count}</td>");
                    messageBuilder.AppendLine($"<td>Price: ${orderDetail.Price}</td>");
                    messageBuilder.AppendLine("</tr>");
                }
                messageBuilder.AppendLine("</table>");

                messageBuilder.AppendLine($"<p>Total Amount: ${orderDetails.First().OrderHeader.OrderTotal}</p>");
                messageBuilder.AppendLine("<p>We apologize for any inconvenience caused by the cancellation.</p>");
                messageBuilder.AppendLine("<p>Thank you for considering Shopping App.</p>");

                // Additional information or instructions can be added as needed

                messageBuilder.AppendLine("<p>Sincerely,</p>");
                messageBuilder.AppendLine("<p>The Book Store App Team</p>");

                // End HTML content
                messageBuilder.AppendLine("</body>");
                messageBuilder.AppendLine("</html>");

                // Use the EmailSender to send the HTML-formatted email
                _emailSender.SendEmailAsync(customerEmail, subject, messageBuilder.ToString());
            }
            catch (Exception ex)
            {
                // Handle email sending failure (log, return error message, etc.)
            }
        }

        [HttpPost]

        public IActionResult DispatchOrder([FromBody] OrderHeader model)
        {
            var fordispatch = _unitOfWork.OrderHeader.FirstOrDefault(x => x.Id == model.Id);

            if (fordispatch == null) return BadRequest("Error dispatching order. Please try again.");
            {
                fordispatch.OrderStatus = SD.OrderStatus_Shipped;
                fordispatch.TrackingNumber = model.TrackingNumber;
                fordispatch.Carrier = model.Carrier;

                _unitOfWork.Save();

                return Ok(new { message = "Order dispatched successfully." });

            }


        }
    }
}
