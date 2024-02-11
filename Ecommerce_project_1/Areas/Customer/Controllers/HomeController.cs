using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Models.ViewModel;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Product = Ecommerce_project_1.Models.Product;

namespace Ecommerce_project_1.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index(string search)
        {
            var claimIdentity=(ClaimsIdentity)User.Identity;
            var claim=claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim!=null)
            {
                var count = _unitOfWork.Cart.GetAll(sc => sc.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.CartSessionCount,count);


            }
            var ProductInDb = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            List<Product> combinedProducts;

            if (!String.IsNullOrEmpty(search))
            {
                var SearchProducts = ProductInDb.Where(n => n.Title.Contains(search) || n.Author.Contains(search)).ToList();
                var relatedProducts = ProductInDb.OrderByDescending(p => p.SalesCount).Take(2).ToList();

                // Combine search results and related products
                combinedProducts = SearchProducts.Concat(relatedProducts).ToList();
            }
            else
            {
                combinedProducts = ProductInDb.OrderByDescending(p => p.SalesCount).ToList();
            }

            return View("Index", combinedProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult Details(int id)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count=_unitOfWork.Cart.GetAll(sc=>sc.ApplicationUserId== claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.CartSessionCount, count);
            }
            var ProductInDb =_unitOfWork.Product.FirstOrDefault(p=>p.Id==id,includeProperties:"Category,CoverType");
            if (ProductInDb == null) return NotFound();

            var shopping_Cart = new Cart()
            {
                ProductId =id,
                Product = ProductInDb,

            };


            return View(shopping_Cart);

        }
        [Authorize]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Details(Cart shopping_Cart)
        {
            shopping_Cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return NotFound();
                shopping_Cart.ApplicationUserId = claim.Value;
                var shoppingCartFromDb = _unitOfWork.Cart.FirstOrDefault(sc => sc.ApplicationUserId == claim.Value && sc.ProductId == shopping_Cart.ProductId);
                if (shoppingCartFromDb == null)
                    _unitOfWork.Cart.Add(shopping_Cart);
                else
                    shoppingCartFromDb.Count += shopping_Cart.Count;
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                var ProductInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shopping_Cart.Id, includeProperties: "Category,CoverType");
                if (ProductInDb == null) return NotFound();

                var shopping_Cart1 = new Cart()
                {
                    ProductId = shopping_Cart.Id,
                    Product = ProductInDb,

                };

                return View(shopping_Cart1);
            }

        }
    }
}



