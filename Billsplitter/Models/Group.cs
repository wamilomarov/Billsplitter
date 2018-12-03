using Microsoft.AspNetCore.Http;

namespace Billsplitter.Models
{
    public class Group
    {
        public string Name { get; set; }
        public IFormFile Photo { get; set; }
        public int CreatedByUserId { get; set; }
        public int CurrencyId { get; set; }
    }
}