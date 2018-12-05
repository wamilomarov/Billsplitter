using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class Purchases
    {
        public Purchases()
        {
            PurchaseMembers = new HashSet<PurchaseMembers>();
        }

        public int Id { get; set; }
        [IgnoreDataMember]
        public int GroupId { get; set; }
        [IgnoreDataMember]
        public int PaidByUserId { get; set; }
//        public string Title { get; set; }
//        public string Comment { get; set; }
        public bool IsComplete { get; set; }
        [IgnoreDataMember]
        public bool Show { get; set; }
        [IgnoreDataMember]
        public int ProductId { get; set; }
//        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }

        [IgnoreDataMember]
        public Groups Group { get; set; }
        [IgnoreDataMember]
        public Users PaidByUser { get; set; }
        public Products Product { get; set; }
        public ICollection<PurchaseMembers> PurchaseMembers { get; set; }
    }
}
