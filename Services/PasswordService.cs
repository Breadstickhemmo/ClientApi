using System.Security.Cryptography;
using System.Text;

namespace MyApiApp.Services
{
    public class PasswordService
    {
        // Метод для хеширования пароля с солью
        public string HashPassword(string password, out string salt)
        {
            using var sha256 = SHA256.Create();
            
            // Генерация соли
            salt = Guid.NewGuid().ToString();

            // Хеширование пароля с солью
            var combinedPassword = Encoding.UTF8.GetBytes(password + salt);
            var hashedBytes = sha256.ComputeHash(combinedPassword);
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string inputPassword, string storedHash, string salt)
        {
            using var sha256 = SHA256.Create();
            var combinedPassword = Encoding.UTF8.GetBytes(inputPassword + salt);
            var hashedBytes = sha256.ComputeHash(combinedPassword);
            var inputHash = Convert.ToBase64String(hashedBytes);

            return inputHash == storedHash;
        }
    }
}
