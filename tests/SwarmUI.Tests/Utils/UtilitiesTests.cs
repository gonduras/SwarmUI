using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("test.png", "image/png")]
        [InlineData("image.jpg", "image/jpeg")]
        [InlineData("data.json", "application/json")]
        [InlineData("script.js", "application/javascript")]
        [InlineData("unknown.xyz", "application/octet-stream")]
        [InlineData("noextension", "application/octet-stream")]
        [InlineData("multiple.dots.in.name.txt", "text/plain")]
        [InlineData(".hiddenfile", "application/octet-stream")]
        [InlineData("path/to/file.png", "image/png")]
        [InlineData("UPPERCASE.PNG", "image/png")]
        [InlineData("", "application/octet-stream")]
        public void GuessContentType_ReturnsCorrectType(string path, string expectedType)
        {
            // Act
            string result = Utilities.GuessContentType(path);

            // Assert
            Assert.Equal(expectedType, result);
        }
    }
}
