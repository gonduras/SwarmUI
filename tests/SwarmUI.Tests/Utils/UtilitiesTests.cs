using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Fact]
        public void GuessContentType_KnownExtension_ReturnsCorrectType()
        {
            Assert.Equal("image/png", Utilities.GuessContentType("image.png"));
            Assert.Equal("image/jpeg", Utilities.GuessContentType("photo.jpg"));
            Assert.Equal("application/json", Utilities.GuessContentType("data.json"));
            Assert.Equal("text/html", Utilities.GuessContentType("index.html"));
        }

        [Fact]
        public void GuessContentType_UnknownExtension_ReturnsOctetStream()
        {
            Assert.Equal("application/octet-stream", Utilities.GuessContentType("file.unknown"));
            Assert.Equal("application/octet-stream", Utilities.GuessContentType("data.xyz123"));
        }

        [Fact]
        public void GuessContentType_NoExtension_ReturnsOctetStream()
        {
            // The string extension logic gets the whole string if there is no '.',
            // and it won't be in the dictionary.
            Assert.Equal("application/octet-stream", Utilities.GuessContentType("filewithoutdot"));
            Assert.Equal("application/octet-stream", Utilities.GuessContentType("anotherfile"));
        }

        [Fact]
        public void GuessContentType_PathWithDirectory_ReturnsCorrectType()
        {
            Assert.Equal("image/png", Utilities.GuessContentType("/var/www/images/image.png"));
            Assert.Equal("text/plain", Utilities.GuessContentType("C:\\temp\\log.txt"));
        }

        [Fact]
        public void GuessContentType_DotInDirectoryButNoExtension_ReturnsOctetStream()
        {
            Assert.Equal("application/octet-stream", Utilities.GuessContentType("/var/www.site/images/file"));
        }

        [Fact]
        public void GuessContentType_CaseSensitivity_ReturnsCorrectTypeForUpper()
        {
            Assert.Equal("image/png", Utilities.GuessContentType("image.PNG"));
        }
    }
}
