using System.ComponentModel.DataAnnotations;

namespace MyApiApp.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        public string Email { get; set; } = "None";

        public string Address { get; set; } = "None";
        public int UserAdd { get; set; }
    }
}
