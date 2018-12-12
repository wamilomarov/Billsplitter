using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using Billsplitter.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Billsplitter.Models
{
    public class User
    {
        private readonly IConfiguration _config;

        public User(IConfiguration config)
        {
            _config = config;
        }

        public User(IConfiguration config, Users userData)
        {
            _config = config;
            Id = userData.Id;
            FullName = userData.FullName;
            Email = userData.Email;
            Photo = userData.Photo;
            CreatedAt = userData.CreatedAt;
        }

        public int Id { get; set; }
        [DataMember(Name = "name")] public string FullName { get; set; }
        [DataMember(Name = "email")] public string Email { get; set; }
        [DataMember(Name = "picture")] public string Photo { get; set; }
        [IgnoreDataMember]
        public string EmailVerificationCode { get; set; }
        public string ApiToken { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime CreatedAt { get; set; }

        public void GenerateToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid, this.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, this.FullName),
                new Claim(JwtRegisteredClaimNames.Email, this.Email),
                new Claim("Photo", this.Photo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: creds);

            ApiToken = new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserLoginModel
    {
        [Required, EmailAddress, MaxLength(255), MinLength(5)]
        public string Email { get; set; }

        [Required, MinLength(6), MaxLength(255)]
        public string Password { get; set; }
    }

    public class UserRegisterModel
    {
        [Required, MaxLength(255)] public string FullName { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        public IFormFile Photo { get; set; }

        [Required, MinLength(6), MaxLength(255)]
        public string Password { get; set; }

        public string GenerateEmailVerificationCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class UserUpdateModel
    {
        [MaxLength(255)] 
        public string FullName { get; set; }
        [EmailAddress, MaxLength(255)] 
        public string Email { get; set; }
        public IFormFile Photo { get; set; }
        [MinLength(6), MaxLength(255)] 
        public string Password { get; set; }
        [MinLength(6), MaxLength(255)] 
        public string OldPassword { get; set; }
    }

    public class PasswordResetModel
    {
        public string Password { get; set; }
        public string Code { get; set; }
    }
}