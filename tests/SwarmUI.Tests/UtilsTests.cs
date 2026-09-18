using System;
using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Fact]
        public void HashPassword_CreatesV2Hash()
        {
            string username = "testuser";
            string password = "testpassword123";

            string hashed = Utilities.HashPassword(username, password);

            Assert.StartsWith("swarmpw_v2:", hashed);
        }

        [Fact]
        public void CompareHashedPassword_SupportsV1Hash()
        {
            string username = "testuser";
            string password = "testpassword123";

            // Generating a v1 hash manually
            byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(128 / 8);
            string borkedPw = $"*SwarmHashedPw:{username}:{password}*";
            byte[] hashed = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                password: borkedPw,
                salt: salt,
                prf: Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256,
                iterationCount: 10_000,
                numBytesRequested: 256 / 8);

            string v1Hash = "swarmpw_v1:" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hashed);

            bool matches = Utilities.CompareHashedPassword(username, password, v1Hash);

            Assert.True(matches);
        }

        [Fact]
        public void CompareHashedPassword_SupportsV2Hash()
        {
            string username = "testuser";
            string password = "testpassword123";

            string v2Hash = Utilities.HashPassword(username, password);

            bool matches = Utilities.CompareHashedPassword(username, password, v2Hash);

            Assert.True(matches);
        }

        [Fact]
        public void CompareHashedPassword_RejectsInvalidPassword()
        {
            string username = "testuser";
            string password = "testpassword123";
            string wrongPassword = "wrongpassword456";

            string v2Hash = Utilities.HashPassword(username, password);

            bool matches = Utilities.CompareHashedPassword(username, wrongPassword, v2Hash);

            Assert.False(matches);
        }
    }
}
