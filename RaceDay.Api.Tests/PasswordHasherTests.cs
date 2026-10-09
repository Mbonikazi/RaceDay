using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class PasswordHasherTests
    {
        private readonly IRaceDayPasswordHasher _hasher = new RaceDayPasswordHasher();

        [Fact]
        public void Hash_ReturnsNonEmptyString()
        {
            var hash = _hasher.Hash("Password123!");
            Assert.False(string.IsNullOrEmpty(hash));
        }

        [Fact]
        public void Hash_DoesNotEqualPlainText()
        {
            var plain = "Password123!";
            var hash = _hasher.Hash(plain);
            Assert.NotEqual(plain, hash);
        }

        [Fact]
        public void Hash_ProducesDifferentHashForDifferentPasswords()
        {
            var hash1 = _hasher.Hash("Password123!");
            var hash2 = _hasher.Hash("DifferentPassword!");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_IsDeterministic_ForSameInput()
        {
            var hash1 = _hasher.Hash("Password123!");
            var hash2 = _hasher.Hash("Password123!");
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Verify_ReturnsTrue_ForCorrectPassword()
        {
            var hash = _hasher.Hash("Password123!");
            Assert.True(_hasher.Verify("Password123!", hash));
        }

        [Fact]
        public void Verify_ReturnsFalse_ForIncorrectPassword()
        {
            var hash = _hasher.Hash("Password123!");
            Assert.False(_hasher.Verify("WrongPassword", hash));
        }

        [Fact]
        public void Verify_IsCaseSensitive()
        {
            var hash = _hasher.Hash("Password123!");
            Assert.False(_hasher.Verify("password123!", hash));
        }
    }
}