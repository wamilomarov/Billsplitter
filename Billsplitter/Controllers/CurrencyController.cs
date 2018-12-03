using System;
using System.Collections.Generic;
using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        
        private readonly billsplitterContext _context;

        public CurrencyController(billsplitterContext billsplitterContext)
        {
            _context = billsplitterContext;
        }
        
        
        // GET
        [HttpGet]
        public IActionResult Get(
            [FromQuery(Name = "q")] string query, 
            [FromQuery(Name = "page")] int page = 1)
        {
            IQueryable<Currencies> currenciesList;
            if (!string.IsNullOrEmpty(query))
            {
                currenciesList = _context.Currencies.OrderBy(c => c.Name)
                    .Where(c => EF.Functions.Like(c.Name, $"%{query}%"));
            }
            else
            {
                currenciesList = _context.Currencies.OrderBy(c => c.Name);
            }
            
            var paginator = new Pagination<Currencies>(currenciesList, page, 20);

            return Ok(paginator.GetPagination());
        }
    }
}