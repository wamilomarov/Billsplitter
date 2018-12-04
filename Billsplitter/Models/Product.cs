using System;
using System.ComponentModel.DataAnnotations;

namespace Billsplitter.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int GroupId { get; set; }
        public string PhotoUrl { get; set; }
        [Required]
        public int Type { get; set; }
        [Required]
        public string BarCode { get; set; }
        [Required]
        public int AddedByUserId { get; set; }
        [Required]
        public int MeasureId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}