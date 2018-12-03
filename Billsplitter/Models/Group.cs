using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Billsplitter.Models
{
    public class Group
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }
        public IFormFile Photo { get; set; }
        public int CreatedByUserId { get; set; }
        [Required]
        public int CurrencyId { get; set; }

        public List<string> Members { get; set; }
    }
}