using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly billsplitterContext _context;

        public GroupController(IConfiguration config, billsplitterContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost, Authorize]
        public IActionResult Post([FromForm] Group request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var imageUploader = new ImageUploader(_config);
            var uploadResult = imageUploader.upload(request.Photo);
            
            if (uploadResult.Error != null)
            {
                ModelState.AddModelError("Photo", uploadResult.Error.Message);
                return BadRequest(ModelState);
            }
            
            var currentUser = HttpContext.User;
            Console.WriteLine("Shamil");
            Console.WriteLine(currentUser);
            Users user = _context.Users
                .FirstOrDefault(u =>
                    u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid).Value));
            Console.WriteLine("Shamil1");

            Groups group = new Groups()
            {
                CurrencyId = request.CurrencyId,
                CreatedByUserId = user.Id,
                Name = request.Name,
                PhotoUrl = uploadResult.PublicId
            };
            Console.WriteLine("Shamil2");

            
            _context.Groups.Add(group);
            _context.SaveChanges();
            
            var groupAdmin = new GroupsUsers()
            {
                UserId = user.Id,
                GroupId = group.Id
            };

            _context.GroupsUsers.Add(groupAdmin);
            _context.SaveChanges();

            foreach (var member in request.Members)
            {
               
                var memberData = _context.Users.FirstOrDefault(u => u.Email == member);
                GroupsUsers groupUser = new GroupsUsers()
                {
                    GroupId = group.Id
                };
                if (memberData == null)
                {
                    Users newUser = new Users()
                    {
                        FullName = member,
                        Email = member
                    };

                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    groupUser.UserId = newUser.Id;
                    //send email
                }
                else
                {
                    groupUser.UserId = memberData.Id;
                }

                _context.GroupsUsers.Add(groupUser);

            }

            _context.SaveChanges();
            

            return Ok();
        }
    }
}