using System;
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
    public class ProductController : ControllerBase
    {
        private readonly billsplitterContext _context;
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config, billsplitterContext context)
        {
            _context = context;
            _config = config;
        }


        [HttpGet("categories"), Authorize]
        public IActionResult Categories([FromQuery] int page = 1)
        {
            var categories = _context.ProductCategories;
            var paginatior = new Pagination<ProductCategories>(categories, page, 10);
            return Ok(paginatior.GetPagination());
        }

        [HttpPost, Authorize]
        public IActionResult Post([FromForm] Product product)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("burda");
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;

            string creatorId = currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid).Value;
            Products addedProduct = null;

            var existingProduct = _context.Products.FirstOrDefault(p => p.BarCode == product.BarCode);
            if (existingProduct != null)
            {
                addedProduct = existingProduct;
            }

            else
            {
                
                var newProduct = new Products()
                {
                    Name = product.Name,
                    BarCode = product.BarCode,
                    CategoryId = product.CategoryId,
                    GroupId = product.GroupId,
                    AddedByUserId = int.Parse(creatorId),
                    MeasureId = 1
                };
                

                if (product.Photo != null)
                {
                    var imageUploader = new ImageUploader(_config);
                    var uploadResult = imageUploader.upload(product.Photo);

                    if (uploadResult.Error != null)
                    {
                        ModelState.AddModelError("Photo", uploadResult.Error.Message);
                        return BadRequest(ModelState);
                    }

                    newProduct.PhotoUrl = uploadResult.PublicId;
                }

                _context.Products.Add(newProduct);
                _context.SaveChanges();
                addedProduct = newProduct;
            }
            
            var purchase = new Purchases()
            {
                ProductId = addedProduct.Id,
                GroupId = addedProduct.GroupId,
                PaidByUserId = addedProduct.AddedByUserId,
                Price = decimal.Parse(HttpContext.Request.Form["price"]),
                IsComplete = false
            };

            _context.Purchases.Add(purchase);

            foreach (var share in product.Shares)
            {
                var purchaseMember = new PurchaseMembers()
                {
                    PurchaseId = purchase.Id,
                    UserId = share,
                    IsPaid = false
                };
                _context.PurchaseMembers.Add(purchaseMember);
            }
            
            _context.SaveChanges();
            
            return Ok();
        }
    }
}