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
            if (request.Members == null)
            {
                request.Members = new List<string>();
            }
            if (!request.Members.Contains(user.Email))
            {
                request.Members.Add(user.Email);
            }
            var members = new HashSet<string>();
            members.UnionWith(request.Members);

            foreach (var member in members)
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
                    //send email if not admin
                }
                else
                {
                    groupUser.UserId = memberData.Id;
                }

                _context.GroupsUsers.Add(groupUser);
            }

            _context.SaveChanges();

            var resultGroup = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.CreatedByUserId == user.Id)
                .FirstOrDefault(g => g.Id == group.Id);

            return Ok(JsonResponse<Groups>.GenerateResponse(resultGroup));
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
                ModelState.AddModelError("Group", "Group can be modified by owner only.");
                return BadRequest(ModelState);
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

            
            
            if (request.Members == null)
            {
                request.Members = new List<string>();
            }
            
            if (!request.Members.Contains(user.Email))
            {
                request.Members.Add(user.Email);
            }
            
            var currentMembers = _context.GroupsUsers.Where(gu => gu.GroupId == group.Id).ToList();
            foreach (var currentMember in currentMembers)
            {
                if (!request.Members.Contains(currentMember.Email)) // if old member is not in new members list
                {
                    _context.GroupsUsers.Remove(currentMember);
                }
            }
            
            var members = new HashSet<string>();
            members.UnionWith(request.Members);

            foreach (var newMember in members)
            {
                
                if (!currentMembers.Exists(cm => cm.Email == newMember)) // if new member email is not in old members
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
                    }
                    else
                    {
                        groupUser.UserId = memberData.Id;
                    }
                    
                    _context.GroupsUsers.Add(groupUser);
                }

                group.Name = request.Name;
                group.CurrencyId = request.CurrencyId;
                _context.Groups.Update(group);
            }

            _context.SaveChanges();
            
            var resultGroup = _context.Groups
                .Include(i => i.Currency)
                .Include(i => i.GroupsUsers)
                .ThenInclude(i => i.User)
                .Where(g => g.CreatedByUserId == user.Id)
                .FirstOrDefault(g => g.Id == id);

            return Ok(JsonResponse<Groups>.GenerateResponse(resultGroup));
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
                .FirstOrDefault(g => g.Id == id && 
                                     g.GroupsUsers
                                         .Any(gu => gu.GroupId == g.Id &&
                                                    gu.UserId == user.Id));

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
                .Where(g => g.GroupsUsers.
                    Any(gu => gu.GroupId == g.Id && 
                              gu.UserId == user.Id));

            var paginator = new Pagination<Groups>(groups, page, 20);

            return Ok(paginator.GetPagination());
        }

        [HttpDelete("{id}"), Authorize]
        public IActionResult Delete(int id)
        {
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
                ModelState.AddModelError("Group", "Group can be deleted by owner only.");
                return BadRequest(ModelState);
            }

            _context.Groups.Remove(group);
            _context.SaveChanges();

            return Ok(new object());
        }

    }
}