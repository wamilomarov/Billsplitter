using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Billsplitter.Entities;
using Microsoft.AspNetCore.Http;

namespace Billsplitter.Models
{
    public class Group
    {
        [Required, MaxLength(255)] public string Name { get; set; }
        public IFormFile Photo { get; set; }
        public int CreatedByUserId { get; set; }
        [Required] public int CurrencyId { get; set; }
        [DefaultValue(default(List<string>))] public List<string> Members { get; set; }
        public Currencies Currency { get; set; }
        public Users CreatedByUser { get; set; }
    }

    public class GroupMoney
    {
        public decimal Value { get; set; }
        public int UserId { get; set; }
    }

    public class GroupOwes
    {
        public List<GroupMoney> IOwe { get; set; }
        public List<GroupMoney> TheyOwe { get; set; }
    }
}