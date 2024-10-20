using System.ComponentModel.DataAnnotations;

namespace MyApiApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        public required string Salt { get; set; }

        public string? Token { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public required string CurrentPassword { get; set; }

        [Required]
        public required string NewPassword { get; set; }
    }
    public class UserLoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }

}
