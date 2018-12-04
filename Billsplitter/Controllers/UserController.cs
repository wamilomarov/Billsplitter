using System;
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
                !string.IsNullOrEmpty(userData.FacebookId))
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
                _context.Users.FirstOrDefault(u => u.FacebookId == googleUser.Sub || u.Email == googleUser.Email);
            User user = null;

            if (check != null)
            {
                check.FacebookId = googleUser.Sub;
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
                    FacebookId = googleUser.Sub,
                    PhotoUrl = uploadResult.PublicId
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                user = new User(_config, newUser);
            }
            
            user.GenerateToken();

            return Ok(JsonResponse<User>.GenerateResponse(user));
        }
    }
}