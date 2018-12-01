using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class Groups
    {
        public Groups()
        {
            GroupsUsers = new HashSet<GroupsUsers>();
            HaveToBuyList = new HashSet<HaveToBuyList>();
            Products = new HashSet<Products>();
            Purchases = new HashSet<Purchases>();
            RepeatingPurchases = new HashSet<RepeatingPurchases>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public int CurrencyId { get; set; }

        public Currencies Currency { get; set; }
        public ICollection<GroupsUsers> GroupsUsers { get; set; }
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        public ICollection<Products> Products { get; set; }
        public ICollection<Purchases> Purchases { get; set; }
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }
    }
}
