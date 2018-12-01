using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class PurchaseMembers
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public sbyte IsPaid { get; set; }

        public Purchases Purchase { get; set; }
        public Users User { get; set; }
    }
}
