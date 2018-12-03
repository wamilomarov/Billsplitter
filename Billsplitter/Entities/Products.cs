using System;
using System.Collections.Generic;

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
        public string PhotoUrl { get; set; }
        public int Type { get; set; }
        public string BarCode { get; set; }
        public int AddedByUserId { get; set; }
        public int MeasureId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Users AddedByUser { get; set; }
        public Groups Group { get; set; }
        public Measures Measure { get; set; }
        public ICollection<HaveToBuyList> HaveToBuyList { get; set; }
        public ICollection<Purchases> Purchases { get; set; }
        public ICollection<RepeatingPurchases> RepeatingPurchases { get; set; }
    }
}
