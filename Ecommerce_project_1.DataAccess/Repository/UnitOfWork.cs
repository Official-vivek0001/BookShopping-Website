using Ecommerce_project_1.DataAccess.Data;
using Ecommerce_project_1.DataAccess.Repository.IRepository;
using Ecommerce_project_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_project_1.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(context);
            CoverType = new CoverTypeRepository(context);
            Product= new ProductRepository(context);
            Company= new CompanyRepository(context);
            ApplicationUser= new ApplicationUserRepository(context);
            Cart= new CartRepository(context);
            OrderDetail= new OrderDetailRepository(context);
            OrderHeader= new OrderHeaderRepository(context);

        }

        public ICategoryRepository Category  {private set;get;}

        public ICoverTypeRepository CoverType { private set; get; }
        public IProductRepository Product { private set; get; }
        public ICompanyRepository Company { private set; get; }
        public IApplicationUserRepository ApplicationUser { private set; get; }
        public ICartRepository Cart { private set; get; }
        public IOrderHeaderRepository OrderHeader { private set; get; }
        public IOrderDetailRepository OrderDetail { private set; get; }

       

        public void Save()
        {
         
           _context.SaveChanges();
        }
    }
}
