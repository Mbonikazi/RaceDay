using System.Security.Cryptography;
using System.Text;

namespace RaceDay.Api.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "RaceDaySalt2024");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
