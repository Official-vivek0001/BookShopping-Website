using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Models.ViewModel;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace Ecommerce_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment= webHostEnvironment;
                
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        public IActionResult GetAll()
        {
            var ProductList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new {data= ProductList });
        }
        public IActionResult Delete(int id)
        {
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null)
            {

                return Json(new { success = false, message = "Something Went Wrong!!!" });
            }
            else
            {
                var webRootPath = _webHostEnvironment.WebRootPath;

                    var imagePath = Path.Combine(webRootPath, productInDb.ImageUrl.Trim('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                
                _unitOfWork.Product.Remove(productInDb);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Data Deleted Successfully!!" });

               
            }
        }

        #endregion
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM=new ProductVM()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()

                }),
                CoverTypeList=_unitOfWork.CoverType.GetAll().Select(cvt=>new SelectListItem()
                {
                    Text = cvt.Name, 
                    Value = cvt.Id.ToString()
                }),               
            Product = new Product()
            };
            if (id == null)
                return View(productVM);
            else
            {
                productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
                return View(productVM);

            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM) 
        { 
            if(ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var files=HttpContext.Request.Form.Files;
                if(files.Count()> 0)
                {
                    var fileName=Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var uploads=Path.Combine(webRootPath,@"images\products");
                    if(productVM.Product.Id!=0)
                    {
                        var imageExist=_unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl=imageExist;
                    }
                    if(productVM.Product.ImageUrl!=null) 
                    {
                        var imagePath=Path.Combine(webRootPath,productVM.Product.ImageUrl.Trim('\\'));
                        if(System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }

                    }
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);

                    }
                    productVM.Product.ImageUrl=Path.Combine(@"\images\products\"+fileName+extension);
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExist = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExist;
                    }

                }
                if (productVM.Product.Id == 0)
                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                productVM = new ProductVM()
                {
                    CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()

                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    Product = new Product()
                };
                if(productVM.Product.Id!=0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);

            }
           

        }
    }


}
