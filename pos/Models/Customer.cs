using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
