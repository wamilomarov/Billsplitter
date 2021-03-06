using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

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
                            p.Show == true &&
                            p.Group.GroupsUsers
                                .Any(gu => gu.UserId == currentUserId &&
                                           gu.GroupId == p.GroupId))
                .Include(p => p.Product)
                .ThenInclude(i => i.Category)
                .Include(p => p.PurchaseMembers)
                .ThenInclude(p => p.User)
                .Include(i => i.PaidByUser)
                .OrderByDescending(p => p.Date);


            var paginator = new Pagination<Purchases>(purchases, page, 10);
            return Ok(paginator.GetPagination());
        }

        [HttpGet("{id}/statistics"), Authorize]
        public IActionResult GroupStatistics(int id, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
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

            var productStatistics = _context.Purchases
                .Where(prc => prc.GroupId == group.Id &&
                              prc.PurchaseMembers.Any(pm => pm.UserId == currentUserId) &&
                              prc.IsComplete == true &&
                              (start == null || prc.Date.Value.Date >= start.Value.Date) &&
                              (end == null || prc.Date.Value.Date <= end.Value.Date))
                .GroupBy(g => g.Product.Category)
                .Select(s => new ProductStatistics()
                {
                    Id = s.Key.Id, 
                    Name = s.Key.Name, 
                    Color = s.Key.Color, 
                    AmountSpent = s.Sum(sum => sum.Price)
                }).ToList();
            
            return Ok(JsonResponse<List<ProductStatistics>>.GenerateResponse(productStatistics));

        }

        [HttpPost("{id}/hide_completed"), Authorize]
        public IActionResult HideCompleted(int id)
        {
            var currentUser = HttpContext.User;

            var currentUserId = int.Parse(currentUser.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);


            Groups group = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.GroupsUsers
                    .Any(gu => gu.GroupId == id &&
                               gu.UserId == currentUserId))
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                ModelState.AddModelError("Group", "You can not edit this group data.");
                return BadRequest(ModelState);
            }

            _context.Purchases
                .Where(p => p.GroupId == group.Id && 
                            p.IsComplete == true &&
                            p.Show == true)
                .ToList()
                .ForEach(i => i.Show = false);

            _context.SaveChanges();

            return Ok(new object());
        }
        
    }
}