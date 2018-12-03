using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Billsplitter.Entities;
using Billsplitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var currentUser = HttpContext.User;

            Users user = _context.Users
                .FirstOrDefault(u =>
                    u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                        .Value));

            Groups group = new Groups()
            {
                CurrencyId = request.CurrencyId,
                CreatedByUserId = user.Id,
                Name = request.Name
            };

            if (request.Photo != null)
            {
                var imageUploader = new ImageUploader(_config);
                var uploadResult = imageUploader.upload(request.Photo);

                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("Photo", uploadResult.Error.Message);
                    return BadRequest(ModelState);
                }

                group.PhotoUrl = uploadResult.PublicId;
            }

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

        [HttpPut("{id}"), Authorize]
        public IActionResult Put(int id, [FromForm] Group request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;

            Users user = _context.Users
                .FirstOrDefault(u =>
                    u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                        .Value));

            Groups group = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.CreatedByUserId == user.Id)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            if (request.Photo != null)
            {
                var imageUploader = new ImageUploader(_config);
                var uploadResult = imageUploader.upload(request.Photo);

                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("Photo", uploadResult.Error.Message);
                    return BadRequest(ModelState);
                }

                group.PhotoUrl = uploadResult.PublicId;
            }

            var currentMembers = _context.GroupsUsers.Where(gu => gu.GroupId == group.Id).ToList();
            foreach (var currentMember in currentMembers)
            {
                _context.GroupsUsers.Remove(currentMember);
            }

            foreach (var newMember in request.Members)
            {
                var memberData = _context.Users.FirstOrDefault(u => u.Email == newMember);
                GroupsUsers groupUser = new GroupsUsers()
                {
                    GroupId = group.Id
                };
                if (memberData == null)
                {
                    Users newUser = new Users()
                    {
                        FullName = newMember,
                        Email = newMember
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

                group.Name = request.Name;
                group.CurrencyId = request.CurrencyId;
                _context.Groups.Update(group);
            }

            _context.SaveChanges();

            return Ok(JsonResponse<Groups>.GenerateResponse(group));
        }

        [HttpGet("{id}"), Authorize]
        public IActionResult Get(int id)
        {
            var currentUser = HttpContext.User;

            Users user = _context.Users.FirstOrDefault(u =>
                u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                    .Value));

            Groups group = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .FirstOrDefault(g => g.Id == id);

            var isMember = _context.GroupsUsers.Count(gu => gu.GroupId == id && gu.UserId == user.Id);


            if (isMember == 0)
            {
                return NotFound();
            }

            return Ok(JsonResponse<Groups>.GenerateResponse(group));
        }
        
        [HttpGet, Authorize]
        public IActionResult Get()
        {
            var currentUser = HttpContext.User;
            var page = !string.IsNullOrEmpty(HttpContext.Request.Query["page"])? 
                Int32.Parse(HttpContext.Request.Query["page"]) :
                1;

            Users user = _context.Users.FirstOrDefault(u =>
                u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                    .Value));

            var groups = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.GroupsUsers.Any(gu => gu.UserId == user.Id));

            var paginator = new Pagination<Groups>(groups, page, 20);

            return Ok(paginator.GetPagination());
        }

        [HttpDelete("{id}"), Authorize]
        public IActionResult Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;

            Users user = _context.Users
                .FirstOrDefault(u =>
                    u.Id == Int32.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)
                        .Value));

            Groups group = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.CreatedByUserId == user.Id)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);

            return Ok();
        }
    }
}