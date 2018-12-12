using System;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class Transactions
    {
        public int Id { get; set; }
        [IgnoreDataMember]
        public int PayerId { get; set; }
        [IgnoreDataMember]
        public int ReceiverId { get; set; }
        [IgnoreDataMember]
        public int GroupId { get; set; }
        
        public decimal Amount { get; set; }
        
        public DateTime CreatedAt { get; set; }

        [IgnoreDataMember]
        public Groups Group { get; set; }
        
        public Users Payer { get; set; }
        
        public Users Receiver { get; set; }
    }
}