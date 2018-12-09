using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;

            var creatorId = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);

            var group = _context.Groups
                .FirstOrDefault(g => g.Id == product.GroupId &&
                                     g.GroupsUsers
                                         .Any(gu => gu.GroupId == g.Id && 
                                                    gu.UserId == creatorId));

            if (group == null)
            {
                ModelState.AddModelError("groupId", "You can not add any record to this group.");
                return BadRequest(ModelState);
            }
            
            Products addedProduct = null;

            Products existingProduct = null;

            if (product.BarCode != null)
            {
                existingProduct = _context.Products.FirstOrDefault(p => p.BarCode == product.BarCode);
            }

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
                    AddedByUserId = creatorId,
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

            return Ok(new object());
        }

        [HttpPut("{id}"), Authorize]
        public IActionResult Put(int id, [FromForm] Product productEdit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var currentUser = HttpContext.User;

            var currentUserId = currentUser.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value;


            var purchase = _context.Purchases
                .Include(i => i.PurchaseMembers)
                .ThenInclude(i => i.User)
                .FirstOrDefault(p => p.Id == id && 
                                     p.Group.GroupsUsers
                                         .Any(gu => gu.UserId == int.Parse(currentUserId) &&
                                                    gu.GroupId == productEdit.GroupId));

            if (purchase == null)
            {
                ModelState.AddModelError("Product", "You can not edit this purchase data.");
                return BadRequest(ModelState);
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == purchase.ProductId);

            if (product != null)
            {
                product.Name = productEdit.Name;
                product.BarCode = productEdit.BarCode;
                product.CategoryId = productEdit.CategoryId;

                _context.Products.Update(product);
            }

            purchase.Price = decimal.Parse(HttpContext.Request.Form["price"]);
            purchase.IsComplete = productEdit.IsComplete;

            _context.Purchases.Update(purchase);

            var currentShares = _context.PurchaseMembers.Where(pm => pm.PurchaseId == purchase.Id).ToList();
            foreach (var currentShare in currentShares)
            {
                _context.PurchaseMembers.Remove(currentShare);
            }

            foreach (var share in productEdit.Shares)
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

            return Ok(purchase);
        }

        [HttpGet, Authorize]
        public IActionResult Get([FromQuery] SearchProduct searchProduct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;
            
            var currentUserId = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);
            
            var product = _context.Products
                .Include(i => i.Category)
                .FirstOrDefault(p => p.BarCode == searchProduct.BarCode 
                                     && p.GroupId == searchProduct.GroupId
                                     && p.Group.GroupsUsers
                                         .Any(gu => gu.GroupId == searchProduct.GroupId &&
                                                    gu.UserId == currentUserId));

            
            return Ok(JsonResponse<Products>.GenerateResponse(product));
        }
    }
}