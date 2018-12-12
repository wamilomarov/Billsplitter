using System;

namespace Billsplitter.Models
{
    public class FacebookUser
    {
        public FacebookUser(dynamic user)
        {
            Sub = user["id"];
            FullName = user["name"];
            Email = user["email"];
            Photo = user["picture"]["data"]["url"];
        }
        public string Sub { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Photo  { get; set; }
    }

    public class FacebookLoginModel
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}