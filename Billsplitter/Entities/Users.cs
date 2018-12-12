using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace Billsplitter.Entities
{
    public partial class Users
    {
        
        private string mailjetApiKey = "8329ba2d79e4645fbf8850c9f0753e46";
        private string mailjetApiSecret = "aaefbd86fba6c42879e7d045ff758d87";
        
        public Users()
        {
            Groups = new HashSet<Groups>();
            GroupsUsers = new HashSet<GroupsUsers>();
            HaveToBuyList = new HashSet<HaveToBuyList>();
            Products = new HashSet<Products>();
            PurchaseMembers = new HashSet<PurchaseMembers>();
            Purchases = new HashSet<Purchases>();
            RepeatingPurchases = new HashSet<RepeatingPurchases>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [IgnoreDataMember]
        public string PhotoUrl { get; set; }
        public string Photo
        {
            get
            {
                if (string.IsNullOrEmpty(PhotoUrl))
                {
                    return "https://res.cloudinary.com/billsplitter/image/upload/default_profile";
                }
                return "https://res.cloudinary.com/billsplitter/image/upload/c_fill,h_150,w_150/" + PhotoUrl;
            }
        }
        
        [IgnoreDataMember]
        public string GoogleId { get; set; }
        [IgnoreDataMember]
        public string FacebookId { get; set; }
        [IgnoreDataMember]
        public string Password { get; set; }
        [IgnoreDataMember]
        public string PasswordResetCode { get; set; }
        [IgnoreDataMember]
        public string EmailVerificationCode { get; set; }
        [IgnoreDataMember]
        public DateTime CreatedAt { get; set; }
        [IgnoreDataMember]
        public ICollection<Groups> Groups { get; set; }
        [IgnoreDataMember]
        public ICollection<GroupsUsers> GroupsUsers { get; set; }
        [IgnoreDataMember]
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        [IgnoreDataMember]
        public ICollection<Products> Products { get; set; }
        [IgnoreDataMember]
        public ICollection<PurchaseMembers> PurchaseMembers { get; set; }
        [IgnoreDataMember]
        public ICollection<Purchases> Purchases { get; set; }
        [IgnoreDataMember]
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }
        
        public string GenerateRandomString()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public async Task<MailjetResponse> SendEmailAsync(string receiver, string subject, string text)
        {
            var client = new MailjetClient(mailjetApiKey, mailjetApiSecret);
            MailjetRequest request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }
                .Property(Send.FromEmail, "service@billsplitter.org")
                .Property(Send.FromName, "Billsplit")
                .Property(Send.Subject, subject)
                .Property(Send.HtmlPart, text)
                .Property(Send.Recipients, new JArray {
                    new JObject {
                        {"Email", receiver}
                    }
                });
            MailjetResponse response = await client.PostAsync(request);

            return response;
        }
    }
}
