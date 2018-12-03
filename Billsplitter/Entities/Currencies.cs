using System;
using System.Collections.Generic;

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

        public ICollection<Groups> Groups { get; set; }
    }
}
