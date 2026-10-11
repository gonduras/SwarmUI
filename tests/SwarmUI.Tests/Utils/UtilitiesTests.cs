using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("Hello World 123", "Hello World 123")]
        [InlineData("Normal string with spaces", "Normal string with spaces")]
        [InlineData("", "")]
        public void EscapeJsonString_NoSpecialChars_ReturnsSameString(string input, string expected)
        {
            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsonString_SpecialCharacters_EscapesCorrectly()
        {
            // Arrange
            string input = "A\\B\"C\nD\rE\bF\tG\fH/I";
            string expected = "A\\\\B\\\"C\\nD\\rE\\bF\\tG\\fH\\/I";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsonString_NonPrintableCharacters_EscapesCorrectly()
        {
            // Arrange
            // \u0001 is a non-printable char < 32
            // \u001F is another one
            string input = "Start\u0001Middle\u001FEnd";
            string expected = "Start\\u0001Middle\\u001FEnd";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsonString_MixedCharacters_EscapesCorrectly()
        {
            // Arrange
            string input = "Test\n\"Value\"\u0002/";
            string expected = "Test\\n\\\"Value\\\"\\u0002\\/";

            // Act
            string result = Utilities.EscapeJsonString(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
