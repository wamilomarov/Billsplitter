using System;
using System.Collections.Generic;

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
        public string PhotoUrl { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string EmailVerificationCode { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Groups> Groups { get; set; }
        public ICollection<GroupsUsers> GroupsUsers { get; set; }
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        public ICollection<Products> Products { get; set; }
        public ICollection<PurchaseMembers> PurchaseMembers { get; set; }
        public ICollection<Purchases> Purchases { get; set; }
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }
    }
}
