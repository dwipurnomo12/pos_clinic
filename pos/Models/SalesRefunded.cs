using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public class SalesRefunded
    {
        [Key]
        public int Id { get; set; }

        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }

        [Required]
        public int StockReturned { get; set; }

        [Required]
        public string Information { get; set; }
    }
}
