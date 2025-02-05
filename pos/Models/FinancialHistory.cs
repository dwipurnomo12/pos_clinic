using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public enum FinanceStatus
    {
        In = 0,
        Out = 1,
    }

    public class FinancialHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public FinanceStatus FinanceStatus { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
