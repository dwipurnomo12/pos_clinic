using System.ComponentModel.DataAnnotations;

namespace pos.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
