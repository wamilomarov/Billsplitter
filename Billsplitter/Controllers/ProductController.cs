using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly billsplitterContext _context;
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config, billsplitterContext context)
        {
            _context = context;
            _config = config;
        }

        [HttpPost, Authorize]
        public IActionResult Post([FromForm] Product product)
        {
            return Ok();
        }
    }
}