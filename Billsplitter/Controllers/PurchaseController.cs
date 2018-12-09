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
        public IActionResult Get(int groupId, [FromQuery] int page = 1)
        {
            var currentUser = HttpContext.User;

            var currentUserId =
                int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);

            var purchases = _context.Purchases
                .Where(p => p.GroupId == groupId &&
                            p.Group.GroupsUsers
                                .Any(gu => gu.UserId == currentUserId &&
                                           gu.GroupId == p.GroupId))
                .Include(p => p.Product)
                .ThenInclude(i => i.Category)
                .Include(p => p.PurchaseMembers)
                .ThenInclude(p => p.User)
                .OrderByDescending(p => p.CreatedAt);


            var paginator = new Pagination<Purchases>(purchases, page, 10);
            return Ok(paginator.GetPagination());
        }

        [HttpGet("{id}/statistics"), Authorize]
        public IActionResult GroupStatistics(int id)
        {
            var currentUser = HttpContext.User;

            var currentUserId = int.Parse(currentUser.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);

            var group = _context.Groups
                .FirstOrDefault(g => g.Id == id &&
                                     g.GroupsUsers.Any(gu => gu.GroupId == g.Id &&
                                                             gu.UserId == currentUserId));

            if (group == null)
            {
                ModelState.AddModelError("Group", "There is no such group, or you are not registered in given group.");
                return BadRequest(ModelState);
            }


            var query = @"SELECT pc.Id, pc.Name, SUM(prc.Price) AS AmountSpent, pc.Color FROM purchases prc
            LEFT JOIN products prd ON prd.Id = prc.ProductId
            LEFT JOIN product_categories pc ON pc.Id = prd.CategoryId
            LEFT JOIN purchase_members pm ON pm.PurchaseId = prc.Id
            WHERE prc.GroupId = 2 AND pm.UserId = 2
            GROUP BY prd.CategoryId";
            var productStatistics = _context.ProductStatistics.FromSql(query).ToList();

            return Ok(JsonResponse<List<ProductStatistics>>.GenerateResponse(productStatistics));

        }
    }
}