using System.ComponentModel.DataAnnotations;

namespace Billsplitter.Models
{
    public class Currency
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
    }
    
}