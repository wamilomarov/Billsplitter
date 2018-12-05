using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class Products
    {
        public Products()
        {
            HaveToBuyList = new HashSet<HaveToBuyList>();
            Purchases = new HashSet<Purchases>();
            RepeatingPurchases = new HashSet<RepeatingPurchases>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        [IgnoreDataMember]
        public string PhotoUrl { get; set; }
        public int CategoryId { get; set; }
        public string BarCode { get; set; }
        [IgnoreDataMember]
        public int AddedByUserId { get; set; }
        [IgnoreDataMember]
        public int MeasureId { get; set; }
        public DateTime CreatedAt { get; set; }

        [IgnoreDataMember]
        public Users AddedByUser { get; set; }
        [IgnoreDataMember]
        public ProductCategories Category { get; set; }
        [IgnoreDataMember]
        public Groups Group { get; set; }
        [IgnoreDataMember]
        public Measures Measure { get; set; }
        [IgnoreDataMember]
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        [IgnoreDataMember]
        public ICollection<Purchases> Purchases { get; set; }
        [IgnoreDataMember]
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }
    }
}
