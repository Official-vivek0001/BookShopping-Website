using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Models.ViewModel;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class RecentOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RecentOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var orders = _unitOfWork.OrderDetail.GetAll(d => d.OrderHeader.ApplicationUserID == claim.Value, includeProperties: "Product,OrderHeader").OrderByDescending(p=>p.OrderHeader.OrderDate).ToList();
            if (orders == null) return NotFound();
            return View(orders);
        }
        [HttpPost]
        [ActionName("BuyAgain")]
        [ValidateAntiForgeryToken]
        public IActionResult BuyAgain(List<int> selectedItems)
        {
            if (selectedItems != null && selectedItems.Any())
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                foreach (var productId in selectedItems)
                {
                    var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == productId);

                    if (productInDb != null)
                    {
                        var shoppingCartFromDb = _unitOfWork.Cart.FirstOrDefault(
                            sc => sc.ApplicationUserId == userId && sc.ProductId == productId
                        );

                        if (shoppingCartFromDb == null)
                        {
                            var shoppingCart = new Cart
                            {
                                ProductId = productId,
                                Count = 1, // You may set the count as needed
                                ApplicationUserId = userId
                            };
                            
                            _unitOfWork.Cart.Add(shoppingCart);
                        }
                        else
                        {
                            shoppingCartFromDb.Count++; // Increase the count if the item is already in the cart
                        }
                    }
                }
                var ClaimIdentidty = (ClaimsIdentity)User.Identity;
                var Claims = ClaimIdentidty.FindFirst(ClaimTypes.NameIdentifier);
                var Count = _unitOfWork.Cart.GetAll(SC => SC.ApplicationUserId == Claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);

                _unitOfWork.Save();

                // Redirect to the cart page
                return RedirectToAction("Index", "Cart");
            }

            // Redirect to the home page if no items are selected
            return RedirectToAction("Index", "Home");
        }

    }
}
