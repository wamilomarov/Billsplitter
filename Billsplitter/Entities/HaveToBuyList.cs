using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class HaveToBuyList
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public int AddedByUserId { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }

        public Users AddedByUser { get; set; }
        public Groups Group { get; set; }
        public Products Product { get; set; }
    }
}
