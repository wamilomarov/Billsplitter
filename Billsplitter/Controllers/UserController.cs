using System.Linq;
using Billsplitter.Entities;
using Billsplitter.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
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
        public IActionResult Register([FromForm]UserRegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(registerModel.Photo.FileName, registerModel.Photo.OpenReadStream()),
            };
            var uploadResult = cloudinary.Upload(uploadParams);

            if (uploadResult.Error != null)
            {
                ModelState.AddModelError("Photo", uploadResult.Error.Message.ToString());
                return BadRequest(ModelState);
            }

            string emailVerificationCode = registerModel.GenerateEmailVerificationCode();
            
            // send async email
            

            Users userData = new Users()
            {
                FullName = registerModel.FullName,
                Email = registerModel.Email,
                PhotoUrl = uploadResult.PublicId,
                Password = new PasswordHasher<UserRegisterModel>().HashPassword(registerModel, registerModel.Password),
                PasswordHash = "aaa",
                EmailVerificationCode = emailVerificationCode
            };

            _context.Users.Add(userData);
            _context.SaveChanges();

            User user = new User(_config)
            {
                Id = userData.Id,
                FullName = userData.FullName,
                Email = userData.Email,
                PhotoUrl = cloudinary.Api.UrlImgUp.Transform(
                    new Transformation().Width(150).Height(150).Crop("fill")).BuildUrl(userData.PhotoUrl),
                EmailVerificationCode = userData.EmailVerificationCode,
                CreatedAt = userData.CreatedAt
            };
            
            user.GenerateToken();

            return Ok(user);
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
                PhotoUrl = cloudinary.Api.UrlImgUp.Transform(
                    new Transformation().Width(150).Height(150).Crop("fill")).BuildUrl(userData.PhotoUrl),
                EmailVerificationCode = userData.EmailVerificationCode,
                CreatedAt = userData.CreatedAt
            };
            
            user.GenerateToken();

            return Ok(user);

        }


    }
}