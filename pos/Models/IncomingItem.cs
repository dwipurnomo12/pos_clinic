﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace pos.Models
{
    public class IncomingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DateOfEntry { get; set; }

        public string? TransactionCode { get; set; }

        public string? BatchNumber { get; set; } 

        [Required]
        public int StockIn { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly ExpiredDate { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalPurchase { get; set; }
    }
}
