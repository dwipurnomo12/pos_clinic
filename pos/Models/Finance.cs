
using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public class Finance
    {
        [Key]
        public int Id { get; set; }

        public decimal Nominal { get; set; }

        public ICollection<FinancialHistory> FinancialHistories { get; set; }
    }
}
