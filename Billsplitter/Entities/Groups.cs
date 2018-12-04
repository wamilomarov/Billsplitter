using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        [IgnoreDataMember]
        public string PhotoUrl { get; set; }
        [IgnoreDataMember]
        public string Photo => "https://res.cloudinary.com/billsplitter/image/upload/c_fill,h_150,w_150/" + PhotoUrl;
        [IgnoreDataMember]
        public int CreatedByUserId { get; set; }
        [IgnoreDataMember]
        public int CurrencyId { get; set; }

        [IgnoreDataMember]
        public Users CreatedByUser { get; set; }
        
        public Currencies Currency { get; set; }
        
        public ICollection<GroupsUsers> GroupsUsers { get; set; }
        [IgnoreDataMember]
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        [IgnoreDataMember]
        public ICollection<Products> Products { get; set; }
        [IgnoreDataMember]
        public ICollection<Purchases> Purchases { get; set; }
        [IgnoreDataMember]
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }

    }
}
