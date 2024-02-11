using Ecommerce_project_1.DataAccess.Data;
using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using Ecommerce_project_1.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Ecommerce_project_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {

            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var User = _context.ApplicationUsers.ToList();
            var RoleList = _context.Roles.ToList();
            var userRole = _context.UserRoles.ToList();
            foreach (var user in User)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = RoleList.FirstOrDefault(r => r.Id == roleId).Name;
                if (user.CompanyId != null)
                {
                  user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32 (user.CompanyId)).Name
                    };
                }
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };

                }
            }
            var adminuser = User.FirstOrDefault(u => u.Role == SD.Role_Admin);
            User.Remove(adminuser);
                return Json(new {data=User});
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            bool isLocked = false;
            var userInDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userInDb == null)
            {
                return Json(new{ success = false, messsage = "Something went wrong!!!"});
            }
            if(userInDb!=null&&userInDb.LockoutEnd>DateTime.Now)
            {
                userInDb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;

            }
            _context.SaveChanges();
            return Json(new { success = true, messsage = isLocked==true?"User Successfully Locked": "User Successfully Unlocked" });


        }
        #endregion
    }
}