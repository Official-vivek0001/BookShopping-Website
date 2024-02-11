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
    public class CartRepository:Repository<Cart>,ICartRepository
    {
        private readonly ApplicationDbContext _context;
        public CartRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
                
        }

    }
}
