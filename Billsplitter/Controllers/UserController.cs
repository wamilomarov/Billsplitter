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

            UploadResult uploadResult = null;
            if (registerModel.Photo != null)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(registerModel.Photo.FileName, registerModel.Photo.OpenReadStream()),
                };
                uploadResult = cloudinary.Upload(uploadParams);

                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("Photo", uploadResult.Error.Message);
                    return BadRequest(ModelState);
                }
            }

           

            string emailVerificationCode = registerModel.GenerateEmailVerificationCode();

            // send async email


            if (userData != null)
            {
                userData.FullName = registerModel.FullName;
                userData.Password =
                    new PasswordHasher<UserRegisterModel>().HashPassword(registerModel, registerModel.Password);
                userData.EmailVerificationCode = emailVerificationCode;
                userData.PhotoUrl = uploadResult?.PublicId;

                _context.Users.Update(userData);
            }
            else
            {
                userData = new Users()
                {
                    FullName = registerModel.FullName,
                    Email = registerModel.Email,
                    PhotoUrl = uploadResult?.PublicId,
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

        [HttpPost("facebook")]
        public async Task<IActionResult> FacebookLoginAsync([FromForm] FacebookLoginModel loginModel)
        {
            var client = new HttpClient();

            var verifyTokenEndPoint =
                string.Format($"https://graph.facebook.com/me?access_token={loginModel.Token}&fields=id,email,name,picture.type(large)");
            var verifyAppEndpoint = string.Format("https://graph.facebook.com/app?access_token={0}", loginModel.Token);

            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);


            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic userObj = (Newtonsoft.Json.Linq.JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                if (loginModel.Email != null)
                {
                    userObj["email"] = loginModel.Email;
                }

                uri = new Uri(verifyAppEndpoint);
                response = await client.GetAsync(uri);
                content = await response.Content.ReadAsStringAsync();
                dynamic appObj = (Newtonsoft.Json.Linq.JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                if (appObj["id"] == _config["Facebook:AppId"])
                {
                    var facebookUser = new FacebookUser(userObj);
                    Users check =
                        _context.Users.FirstOrDefault(u => u.FacebookId == facebookUser.Sub || u.Email == facebookUser.Email);
                    User user = null;
                    
                    if (check != null)
                    {
                        check.FacebookId = facebookUser.Sub;
                        _context.Users.Update(check);
                        _context.SaveChanges();
                        user = new User(_config, check);
                    }
                    else
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(facebookUser.Photo)
                        };

                        var uploadResult = cloudinary.Upload(uploadParams);

                        Users newUser = new Users()
                        {
                            FullName = facebookUser.FullName,
                            Email = facebookUser.Email,
                            FacebookId = facebookUser.Sub,
                            PhotoUrl = uploadResult.PublicId
                        };

                        _context.Users.Add(newUser);
                        _context.SaveChanges();
                        user = new User(_config, newUser);
                        
                    }
                    
                    user.GenerateToken();

                    return Ok(JsonResponse<User>.GenerateResponse(user));
                }
                else
                {
                    ModelState.AddModelError("Facebook", "Please login with right application.");
                    return BadRequest(ModelState);
                }
                
            }
            else
            {
                ModelState.AddModelError("Facebook", "Please provide correct credentials for Facebook account.");
                return BadRequest(ModelState);
            }
            
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
        
        [HttpGet("me"), Authorize]
        public IActionResult Get()
        {
            var currentUser = HttpContext.User;

            var currentUserId = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sid)?.Value);
            
            var user = _context.Users.Find(currentUserId);

            return Ok(JsonResponse<Users>.GenerateResponse(user));

        }

        [HttpPost("forgot_password")]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            
            var userData = _context.Users.FirstOrDefault(u => u.Email == email);

            if (userData == null)
            {
                ModelState.AddModelError("User", "There is no user with given email.");
                return BadRequest(ModelState);
            }
            userData.PasswordResetCode = userData.GenerateRandomString();
            
            var user = new User(_config, userData);
            
            string html = $@"<h3>Dear {user.FullName}, we got your request to reset password.</h3>
                            <p>Here is your password reset code, please use it to set new password.</p>";
            var send = user.SendEmailAsync(user.Email, "Password Reset", html);
            
            _context.Users.Update(userData);
            _context.SaveChanges();

            await send;

            return Ok(new object());
        }

        [HttpPost("reset_password")]
        public IActionResult ResetPassword([FromForm] PasswordResetModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.PasswordResetCode == request.Code);

            if (user == null)
            {
                ModelState.AddModelError("Code", "Please try again with the correct reset code.");
                return BadRequest(ModelState);
            }

            user.Password = new PasswordHasher<PasswordResetModel>().HashPassword(request,
                request.Password);

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new object());

        }
    }
}