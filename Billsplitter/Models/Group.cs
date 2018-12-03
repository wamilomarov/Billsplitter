using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Billsplitter.Entities;
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
        [Required]
        public List<string> Members { get; set; }
        public Currencies Currency { get; set; }
        public Users CreatedByUser { get; set; }
    }
}