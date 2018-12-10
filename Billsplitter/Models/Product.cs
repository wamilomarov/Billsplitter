using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Billsplitter.Entities;
using Microsoft.AspNetCore.Http;

namespace Billsplitter.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int GroupId { get; set; }
        public int PaidById { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public IFormFile Photo { get; set; }
        public string BarCode { get; set; }
        public int AddedByUserId { get; set; }
        public int MeasureId { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public IEnumerable<int> Shares { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime? Date { get; set; }
        
        public bool IsComplete { get; set; }

    }

    public class SearchProduct
    {
        [Required, MaxLength(255)]
        public string BarCode { get; set; }
        [Required]
        public int GroupId { get; set; }
    }

    public class ProductStatistics
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal AmountSpent { get; set; }
        public string Color { get; set; }
    }
}