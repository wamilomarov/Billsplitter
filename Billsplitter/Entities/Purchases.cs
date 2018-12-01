using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class Purchases
    {
        public Purchases()
        {
            PurchaseMembers = new HashSet<PurchaseMembers>();
        }

        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PaidByUserId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public sbyte IsComplete { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }

        public Groups Group { get; set; }
        public Users PaidByUser { get; set; }
        public Products Product { get; set; }
        public ICollection<PurchaseMembers> PurchaseMembers { get; set; }
    }
}
