using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class GroupsUsers
    {
        
        public int Id { get; set; }
        
        public int UserId { get; set; }
        [IgnoreDataMember]
        public int GroupId { get; set; }

        [IgnoreDataMember]
        public Groups Group { get; set; }
        [IgnoreDataMember]
        public Users User { get; set; }

        public virtual string FullName => User.FullName;
        public virtual string Email => User.Email;
        public virtual string Photo => User.Photo;
    }
}
