using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Ecommerce_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
                _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
#region APIs
        [HttpGet]
        public IActionResult GetAll() {
            var coverTypeList=_unitOfWork.CoverType.GetAll();
            return Json(new {data=coverTypeList});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var coverTypeInDb = _unitOfWork.CoverType.Get(id);
            if(coverTypeInDb==null)
                return Json(new {success=false,message="Something Went Wrong!!!"});
            _unitOfWork.CoverType.Remove(coverTypeInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
        }
        #endregion        
        public IActionResult Upsert(int? id)
        {
            CoverType coverType=new CoverType();
            if(id==null)
                return View(coverType);
            coverType=_unitOfWork.CoverType.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            if(coverType==null) return NotFound();
            if(!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitOfWork.CoverType.Add(coverType);
            else
                _unitOfWork.CoverType.Update(coverType);
            _unitOfWork.Save();
            return RedirectToAction("Index");

        }
    }

}
