using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class PurchaseController : Controller
    {
        
        private readonly billsplitterContext _context;
        private readonly IConfiguration _config;

        public PurchaseController(IConfiguration config, billsplitterContext context)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("{groupId}"), Authorize]
        public IActionResult Get(int groupId, [FromQuery]int page = 1)
        {
            var purchases = _context.Purchases.Where(p => p.GroupId == groupId)
                .Include(p => p.Product)
                .Include(p => p.PurchaseMembers)
                .ThenInclude(p => p.User)
                .OrderByDescending(p => p.CreatedAt);

            
            var paginator = new Pagination<Purchases>(purchases, page, 10);
            return Ok(paginator.GetPagination());
        }
    }
}