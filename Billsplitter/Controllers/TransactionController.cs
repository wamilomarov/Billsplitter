using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly IConfiguration _config;

        private readonly billsplitterContext _context;

        public TransactionController(IConfiguration configuration, billsplitterContext context)
        {
            _config = configuration;
            _context = context;
        }

        [HttpGet("{id}/money"), Authorize]
        public IActionResult Money(int id)
        {
            var currentHttpUser = HttpContext.User;

            var currentUser = _context.Users
                .FirstOrDefault(u =>
                    u.Id == int.Parse(currentHttpUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                        .Value));

            var group = _context.Groups
                .FirstOrDefault(g => g.Id == id &&
                                     g.GroupsUsers
                                         .Any(gu => gu.UserId == currentUser.Id &&
                                                    gu.GroupId == g.Id));
            
            var result = new GroupOwes();
            
            var handler = new Money(_context, group.Id, currentUser.Id);

            result.IOwe = handler.Owe();
            result.TheyOwe = handler.Paid();
            
            return Ok(JsonResponse<GroupOwes>.GenerateResponse(result));
        }
    }
}