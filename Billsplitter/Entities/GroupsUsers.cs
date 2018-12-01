using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class GroupsUsers
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }

        public Groups Group { get; set; }
        public Users User { get; set; }
    }
}
