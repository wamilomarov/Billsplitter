using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class PurchaseMembers
    {
        public int Id { get; set; }

        [IgnoreDataMember]
        public int PurchaseId { get; set; }
//        [IgnoreDataMember]
        public int UserId { get; set; }
        [IgnoreDataMember]
        public bool IsPaid { get; set; }

        [IgnoreDataMember]
        public Purchases Purchase { get; set; }
        [IgnoreDataMember]
        public Users User { get; set; }

        public string FullName => User.FullName;
        public string Email => User.Email;
        public string Photo => User.Photo;
    }
}
