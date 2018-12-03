using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Billsplitter.Entities
{
    public partial class Currencies
    {
        public Currencies()
        {
            Groups = new HashSet<Groups>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        
        [IgnoreDataMember]
        public ICollection<Groups> Groups { get; set; }
    }
}
