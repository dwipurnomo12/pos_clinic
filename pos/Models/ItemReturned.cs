using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public class ItemReturned
    {
        [Key]
        public int Id { get; set; }

        public int IncomingItemId { get; set; }
        public IncomingItem? IncomingItem { get; set; }

        [Required]
        public int StockReturned { get; set; }

        [Required]
        public string Information { get; set; }
    }
}
