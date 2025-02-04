using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public enum TransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Canceled = 2
    }

    public enum PaymentMethod
    {
        Cash = 0,
        OnlinePayment = 1,
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public string? TransactionCode { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
