using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Billsplitter.Entities;
using Billsplitter.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly billsplitterContext _context;
        private readonly Account cloudinaryAccount;
        private readonly Cloudinary cloudinary;


        public UserController(IConfiguration config, billsplitterContext context)
        {
            _config = config;
            _context = context;
            cloudinaryAccount = new Account(
                _config["Cloudinary:CloudName"],
                _config["Cloudinary:ApiKey"],
                _config["Cloudinary:ApiSecret"]);
            cloudinary = new Cloudinary(this.cloudinaryAccount);
        }


        [HttpPost("register")]
        public IActionResult Register([FromForm] UserRegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Users userData = _context.Users.FirstOrDefault(u => u.Email == registerModel.Email);

            if (userData != null && !string.IsNullOrEmpty(userData.Password) &&
                !string.IsNullOrEmpty(userData.GoogleId))
            {
                ModelState.AddModelError("Email", "This email is already taken");
                return BadRequest(ModelState);
            }

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(registerModel.Photo.FileName, registerModel.Photo.OpenReadStream()),
            };
            var uploadResult = cloudinary.Upload(uploadParams);

            if (uploadResult.Error != null)
            {
                ModelState.AddModelError("Photo", uploadResult.Error.Message);
                return BadRequest(ModelState);
            }

            string emailVerificationCode = registerModel.GenerateEmailVerificationCode();

            // send async email


            if (userData != null)
            {
                userData.FullName = registerModel.FullName;
                userData.Password =
                    new PasswordHasher<UserRegisterModel>().HashPassword(registerModel, registerModel.Password);
                userData.EmailVerificationCode = emailVerificationCode;
                userData.PhotoUrl = uploadResult.PublicId;

                _context.Users.Update(userData);
            }
            else
            {
                userData = new Users()
                {
                    FullName = registerModel.FullName,
                    Email = registerModel.Email,
                    PhotoUrl = uploadResult.PublicId,
                    Password = new PasswordHasher<UserRegisterModel>().HashPassword(registerModel,
                        registerModel.Password),
                    EmailVerificationCode = emailVerificationCode
                };

                _context.Users.Add(userData);
            }

            _context.SaveChanges();

            User user = new User(_config)
            {
                Id = userData.Id,
                FullName = userData.FullName,
                Email = userData.Email,
                Photo = cloudinary.Api.UrlImgUp.Transform(
                    new Transformation().Width(150).Height(150).Crop("fill")).BuildUrl(userData.PhotoUrl),
                EmailVerificationCode = userData.EmailVerificationCode,
                CreatedAt = userData.CreatedAt
            };

            user.GenerateToken();

            return Ok(JsonResponse<User>.GenerateResponse(user));
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] UserLoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Users userData = _context.Users.FirstOrDefault(u => u.Email == loginModel.Email);

            if (userData == null)
            {
                ModelState.AddModelError("Email", "This email is not registered in our system");
                return BadRequest(ModelState);
            }

            PasswordVerificationResult passwordCheck = new PasswordHasher<UserLoginModel>()
                .VerifyHashedPassword(loginModel, userData.Password, loginModel.Password);

            if (passwordCheck == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Auth", "Credential are wrong, please try again with correct ones.");
                return BadRequest(ModelState);
            }

            User user = new User(_config)
            {
                Id = userData.Id,
                FullName = userData.FullName,
                Email = userData.Email,
                Photo = cloudinary.Api.UrlImgUp.Transform(
                    new Transformation().Width(150).Height(150).Crop("fill")).BuildUrl(userData.PhotoUrl),
                EmailVerificationCode = userData.EmailVerificationCode,
                CreatedAt = userData.CreatedAt
            };

            user.GenerateToken();

            return Ok(JsonResponse<User>.GenerateResponse(user));
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLoginAsync([FromForm] string token)
        {
            var query = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + token;

            var client = new HttpClient();
            string response = await client.GetStringAsync(query);

            var googleUser = JsonConvert.DeserializeObject<GoogleUser>(response);

//            return Ok(googleUser);

            Users check =
                _context.Users.FirstOrDefault(u => u.GoogleId == googleUser.Sub || u.Email == googleUser.Email);
            User user = null;

            if (check != null)
            {
                check.GoogleId = googleUser.Sub;
                _context.Users.Update(check);
                _context.SaveChanges();
                user = new User(_config, check);
            }
            else
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(googleUser.Picture)
                };

                var uploadResult = cloudinary.Upload(uploadParams);


                Users newUser = new Users()
                {
                    FullName = googleUser.Name,
                    Email = googleUser.Email,
                    GoogleId = googleUser.Sub,
                    PhotoUrl = uploadResult.PublicId
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                user = new User(_config, newUser);
            }
            
            user.GenerateToken();

            return Ok(JsonResponse<User>.GenerateResponse(user));
        }

        [HttpPut("me"), Authorize]
        public IActionResult Put(UserUpdateModel userUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User;

            var currentUserId = currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value;
            
            var user = _context.Users.Find(int.Parse(currentUserId));

            if (!string.IsNullOrEmpty(userUpdate.Email))
            {
                var emailExists = _context.Users.Count(u => u.Email == userUpdate.Email && u.Id != user.Id);
                if (emailExists > 0)
                {
                    ModelState.AddModelError("Email", "This email is already taken.");
                    return BadRequest(ModelState);
                }
                
                user.Email = userUpdate.Email.ToLower();
            }

            if (!string.IsNullOrEmpty(userUpdate.FullName))
            {
                user.FullName = userUpdate.FullName;
            }

            if (userUpdate.Photo != null)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(userUpdate.Photo.FileName, userUpdate.Photo.OpenReadStream()),
                };
                var uploadResult = cloudinary.Upload(uploadParams);

                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("Photo", uploadResult.Error.Message);
                    return BadRequest(ModelState);
                }

                user.PhotoUrl = uploadResult.PublicId;
            }

            if (!string.IsNullOrEmpty(userUpdate.Password) && !string.IsNullOrWhiteSpace(userUpdate.OldPassword))
            {
                PasswordVerificationResult passwordCheck = new PasswordHasher<UserUpdateModel>()
                    .VerifyHashedPassword(userUpdate, user.Password, userUpdate.Password);

                if (passwordCheck == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("OldPassword", "Ypu typed wrong Old Password, please try again with correct one.");
                    return BadRequest(ModelState);
                }
                user.Password = new PasswordHasher<UserUpdateModel>().HashPassword(userUpdate, userUpdate.Password);
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(JsonResponse<Users>.GenerateResponse(user));

        }
    }
}