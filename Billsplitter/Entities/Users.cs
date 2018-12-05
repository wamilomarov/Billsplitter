using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class Users
    {
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
        public string Password { get; set; }
        [IgnoreDataMember]
        public string PasswordHash { get; set; }
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
    }
}
