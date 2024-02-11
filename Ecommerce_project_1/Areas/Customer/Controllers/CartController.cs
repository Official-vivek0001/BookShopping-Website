using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Models.ViewModel;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace Ecommerce_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private static bool IsEmailConfirm = false;
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender,UserManager<IdentityUser>userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            var claimIdentity=(ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<Cart>()
                };
                return View(ShoppingCartVM);
            }

            var count = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == claim.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.CartSessionCount, count);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader(),
               
                
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser=_unitOfWork.ApplicationUser.FirstOrDefault(au=>au.Id==claim.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price=SD.GetPriceBasedOnQuantity(list.Count,list.Price,list.Product.Price50,list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal+=( list.Price*list.Count);
                if(list.Product.Description.Length>100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 100) + "...";
                }
            }
            //Email
            if(!IsEmailConfirm)
            {
                ViewBag.EmailMessage = "Email Has Been Sent Kindly Verify your Email ";
                ViewBag.EmailCSS = "text-success";
                IsEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email Must Be Verified!!! ";
                ViewBag.EmailCSS = "text-danger";
            }
            return View(ShoppingCartVM);
        }
        public async Task< IActionResult> IndexPost()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claim.Value);
            if (user == null)
                ModelState.AddModelError(string.Empty, "email emplty!!!");
            else
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity" ,userId = userId, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                IsEmailConfirm= true;
                
            }
            return RedirectToAction("Index");
            

        }
        public IActionResult Increment(int id)
        {
            var productInDb = _unitOfWork.Cart.Get(id);
            if (productInDb == null) return NotFound();
            productInDb.Count+=1;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Decrement(int id)
        {
            var productInDb = _unitOfWork.Cart.Get(id);
            if (productInDb == null) return NotFound();
            if(productInDb.Count == 0)
            {
                _unitOfWork.Cart.Remove(productInDb);
                _unitOfWork.Save();
                return RedirectToAction("Index","Home");



            }
            productInDb.Count--;
            _unitOfWork.Save();
            return RedirectToAction("Index","Home");
        }
        public IActionResult Delete(int id)
        {
            var productInDb = _unitOfWork.Cart.Get(id);
            if (productInDb == null) return NotFound();
            if (productInDb.Count == 0)
            {
                return RedirectToAction("Index");
            }
            _unitOfWork.Cart.Remove(productInDb);
            _unitOfWork.Save();


            return RedirectToAction("Index");
        }
        public IActionResult Summary( int[] isChecked)
        {
            var claimIdentity=(ClaimsIdentity)User.Identity;
            var Claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart=new List<Cart>()
                };
                return View(ShoppingCartVM);
            }
                
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == Claims.Value && isChecked.Contains(sc.ProductId),includeProperties: "Product"),


                OrderHeader = new OrderHeader(),
                UserAddresses = new List<Models.Address>()


            };

            var orderHeaders = _unitOfWork.OrderHeader.GetAll(SC => SC.ApplicationUserID == Claims.Value).ToList();

            ShoppingCartVM.UserAddresses = orderHeaders.Select(x => new Models.Address

            {
                Name = x.Name,
                City = x.City,
                StreetAddress = x.StreetAddress,
                PhoneNumber = x.PhoneNumber,
                PostalCode = x.PostalCode,
                State = x.State,
                Id = x.Id,
            }).DistinctBy(x => x.FullAddress).ToList();


            //var productlist = ShoppingCartVM.ListCart.Where(a => a.ProductId.ToString().Contains(ProductIds.ToString()));
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claims.Value);
            ShoppingCartVM.OrderHeader.OrderTotal = 0;


              foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 100) + "...";
                }
               
            }

              ShoppingCartVM.OrderHeader.Name=ShoppingCartVM.OrderHeader.ApplicationUser.Name;
              ShoppingCartVM.OrderHeader.StreetAddress=ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
              ShoppingCartVM.OrderHeader.City=ShoppingCartVM.OrderHeader.ApplicationUser.City;

              ShoppingCartVM.OrderHeader.State=ShoppingCartVM.OrderHeader.ApplicationUser.State;
              ShoppingCartVM.OrderHeader.PhoneNumber=ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
              ShoppingCartVM.OrderHeader.PostalCode=ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ViewBag.isChecked = isChecked;
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("VerifyEmailAjax")]
        public async Task<IActionResult> VerifyEmailAjax()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claim.Value);
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                IsEmailConfirm = true;

                return Json(new { success = true, message = "Email verification initiated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's requirements
                return Json(new { success = false, message = "Error in email verification: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ActionName("Summary")]
        public async Task<IActionResult> SummaryPostAsync(int[] isChecked,string StripeToken)
        {

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var Claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claims.Value);
            ShoppingCartVM.ListCart = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == Claims.Value && isChecked.Contains(sc.ProductId), includeProperties: "Product");
            
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatus_Processing;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_Pending;
            ShoppingCartVM.OrderHeader.OrderDate =DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserID =Claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
           
            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
                item.Product.SalesCount++;
                OrderDetail orderDetail = new OrderDetail()
                {

                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = item.ProductId,
                    Price= item.Price,
                    Count= item.Count,
                };
                //ShoppingCartVM.OrderHeader.OrderTotal += (item.Count * item.Price);
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();

            }
            _unitOfWork.Cart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

           
            var count = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.CartSessionCount, count);

            #region Stripe
            if (StripeToken== null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate= DateTime.Today.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_Delayed;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatus_Approved;

            }
            else
            {
                //payment Process
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order Id :" + ShoppingCartVM.OrderHeader.Id,
                    Source = StripeToken

                };
                var Service = new ChargeService();
                Charge charge = Service.Create(options);
                if (charge.BalanceTransactionId == null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_Rejected;

                    string subject = "Payment Failed - Shopping App";
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine($"Dear {ShoppingCartVM.OrderHeader.ApplicationUser.Name},");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Alert Payment Failed for placing an order with Shopping App. Your order details are as follows:");

                    foreach (var orderDetail in ShoppingCartVM.ListCart)
                    {
                        messageBuilder.AppendLine($"Order Id: {ShoppingCartVM.OrderHeader.Id}");
                        messageBuilder.AppendLine($"Product: {orderDetail.Product.Title}");
                        messageBuilder.AppendLine($"Quantity: {orderDetail.Count}");
                        messageBuilder.AppendLine($"Price: ${orderDetail.Price}");
                        messageBuilder.AppendLine();
                    }

                    messageBuilder.AppendLine($"Total Amount: ${ShoppingCartVM.OrderHeader.OrderTotal} Has Been Failed Please Try Again");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Thank you for shopping with us!"); 
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Sincerely,");
                    messageBuilder.AppendLine("The Book store App Team");

                    // Use the EmailSender to send the email
                    await _emailSender.SendEmailAsync(ShoppingCartVM.OrderHeader.ApplicationUser.Email, subject, messageBuilder.ToString());
                }
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatus_Approved;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatus_Approved;
                   
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

                    // Send well-structured order confirmation email to the customer
                    string subject = " Order Confirmation - Shopping App";
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("<!DOCTYPE html>");
                    messageBuilder.AppendLine("<html lang=\"en\">");
                    messageBuilder.AppendLine("<head>");
                    messageBuilder.AppendLine("<meta charset=\"UTF-8\">");
                    messageBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    messageBuilder.AppendLine("<title>Order Confirmed</title>");
                    messageBuilder.AppendLine("</head>");
                    messageBuilder.AppendLine("<body>");
                    messageBuilder.AppendLine($"Dear {ShoppingCartVM.OrderHeader.ApplicationUser.Name},");
                    messageBuilder.AppendLine("<p>Thank you for placing an order with Shopping App. Your order details are as follows:<p>");

                    messageBuilder.AppendLine("<table border=\"1\">");

                    foreach (var orderDetail in ShoppingCartVM.ListCart)
                    {
                        messageBuilder.AppendLine("<tr>");
                        messageBuilder.AppendLine($"<td>Order Id: {ShoppingCartVM.OrderHeader.Id}</td>");
                        messageBuilder.AppendLine($"<td>Product: {orderDetail.Product.Title}</td>");
                        messageBuilder.AppendLine($"<td>Quantity: {orderDetail.Count}</td>");
                        messageBuilder.AppendLine($"<td>Price: ${orderDetail.Price}</td>");
                        messageBuilder.AppendLine("</tr>");
                    }
                    messageBuilder.AppendLine("</table>");

                    messageBuilder.AppendLine($"<p>Total Amount: ${ShoppingCartVM.OrderHeader.OrderTotal}</p>");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("<p>Thank you for shopping with us!</p>");
                    messageBuilder.AppendLine();
                   
                    messageBuilder.AppendLine("<p>Sincerely,</p>");
                    messageBuilder.AppendLine("<p>The Book Store App Team</p>");

                    // End HTML content
                    messageBuilder.AppendLine("</body>");
                    messageBuilder.AppendLine("</html>");

                    // Use the EmailSender to send the email
                    await _emailSender.SendEmailAsync(ShoppingCartVM.OrderHeader.ApplicationUser.Email, subject, messageBuilder.ToString());
                }
                _unitOfWork.Save();


            }
            #endregion

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        private string GenerateOrderDetailsForSMS(ShoppingCartVM shoppingCartVM)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Order Confirmation - Shopping App");
            sb.AppendLine($"Dear {shoppingCartVM.OrderHeader.ApplicationUser.Name},");
            sb.AppendLine($"Thank you for placing an order with Shopping App. Your order details are as follows:");

            foreach (var orderDetail in shoppingCartVM.ListCart)
            {
                sb.AppendLine($"Order ID:{shoppingCartVM.OrderHeader.Id},Product: {orderDetail.Product.Title}, Quantity: {orderDetail.Count}, Price: ${orderDetail.Price}");
            }

            sb.AppendLine($"Total Amount: ${shoppingCartVM.OrderHeader.OrderTotal}");
            sb.AppendLine("Thank you for shopping with us!");

            return sb.ToString();
        }
        public async Task< IActionResult> OrderConfirmation(int id)
        {
            
                return View(id);
        }
    }
}
