using System;
using System.Collections.Generic;

namespace Billsplitter.Entities
{
    public partial class Measures
    {
        public Measures()
        {
            Products = new HashSet<Products>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Products> Products { get; set; }
    }
}
