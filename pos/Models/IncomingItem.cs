using System.ComponentModel.DataAnnotations;

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
    }
}
