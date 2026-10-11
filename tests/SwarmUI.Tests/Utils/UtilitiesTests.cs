using Xunit;
using SwarmUI.Utils;
using System.Text;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("valid_name.txt", "valid_nametxt")]
        [InlineData("path/to/file.txt", "path/to/filetxt")]
        [InlineData("invalid*name?.txt", "invalidnametxt")]
        [InlineData("test//file.txt", "test/filetxt")]
        [InlineData("///test.txt", "testtxt")]
        [InlineData("con", "con_")]
        [InlineData("PRN", "PRN_")]
        [InlineData("a/aux/b", "a/aux_/b")]
        [InlineData("valid\\path\\file", "valid/path/file")]
        public void StrictFilenameClean_ValidNames(string input, string expected)
        {
            var result = Utilities.StrictFilenameClean(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("hello world", "68656c6c6f20776f726c64")]
        [InlineData("", "")]
        [InlineData("test", "74657374")]
        public void BytesToHex_ReturnsCorrectHex(string input, string expected)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var result = Utilities.BytesToHex(bytes);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CombinePathWithAbsolute_TwoArgs_CorrectlyCombines()
        {
            string a = "folderA";
            string b = "folderB";

            var result = Utilities.CombinePathWithAbsolute(a, b);

            result = result.Replace('\\', '/');
            Assert.Equal("folderA/folderB", result);
        }

        [Fact]
        public void CombinePathWithAbsolute_ThreeArgs_CorrectlyCombines()
        {
            string a = "folderA";
            string b = "folderB";
            string c = "folderC";

            var result = Utilities.CombinePathWithAbsolute(a, b, c);

            result = result.Replace('\\', '/');
            Assert.Equal("folderA/folderB/folderC", result);
        }

        [Fact]
        public void HashPassword_And_CompareHashedPassword_Match()
        {
            string username = "testuser";
            string password = "testpassword123!";

            string hash = Utilities.HashPassword(username, password);

            Assert.NotNull(hash);
            Assert.StartsWith("swarmpw_v1:", hash);

            bool match = Utilities.CompareHashedPassword(username, password, hash);
            Assert.True(match);
        }

        [Fact]
        public void CompareHashedPassword_IncorrectPassword_ReturnsFalse()
        {
            string username = "testuser";
            string password = "testpassword123!";
            string wrongPassword = "wrongpassword!";

            string hash = Utilities.HashPassword(username, password);

            bool match = Utilities.CompareHashedPassword(username, wrongPassword, hash);
            Assert.False(match);
        }

        [Fact]
        public void SplitStandardCsv_ValidInput_SplitsCorrectly()
        {
            string input = "a,b,c";
            var result = Utilities.SplitStandardCsv(input);

            Assert.Equal(3, result.Length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
            Assert.Equal("c", result[2]);
        }

        [Fact]
        public void SplitStandardCsv_WithQuotes_SplitsCorrectly()
        {
            string input = "a,\"b,c\",d";
            var result = Utilities.SplitStandardCsv(input);

            Assert.Equal(3, result.Length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b,c", result[1]);
            Assert.Equal("d", result[2]);
        }

        [Fact]
        public void SplitStandardCsv_WithEscapedQuotes_SplitsCorrectly()
        {
            string input = "a,\"b\"\"c\",d";
            var result = Utilities.SplitStandardCsv(input);

            Assert.Equal(3, result.Length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b\"c", result[1]);
            Assert.Equal("d", result[2]);
        }
    }
}
