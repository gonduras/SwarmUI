using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Fact]
        public void EscapeJsonString_NormalString_NoEscapeNeeded()
        {
            // Arrange
            string input = "Hello, world!";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void EscapeJsonString_ContainsQuotes_EscapesCorrectly()
        {
            // Arrange
            string input = "Hello \"world\"!";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal("Hello \\\"world\\\"!", result);
        }

        [Fact]
        public void EscapeJsonString_ContainsNewlines_EscapesCorrectly()
        {
            // Arrange
            string input = "Hello\nworld\r";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal("Hello\\nworld\\r", result);
        }

        [Fact]
        public void EscapeJsonString_ContainsControlCharacters_EscapesCorrectly()
        {
            // Arrange
            string input = "Hello\tworld\b\f\\/";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal("Hello\\tworld\\b\\f\\\\\\/", result);
        }

        [Fact]
        public void EscapeJsonString_ContainsNonPrintableAscii_EscapesCorrectly()
        {
            // Arrange
            string input = "Hello\x00world\x1F!";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal("Hello\\u0000world\\u001F!", result);
        }
    }
}
