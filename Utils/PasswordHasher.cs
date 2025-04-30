using System.Security.Cryptography;
using System.Text;

namespace AuthService.Utils
{
    public class PasswordHasher
    {
        public static string Hash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string salt, string hash)
        {
            return Hash(password, salt) == hash;
        }
    }
}