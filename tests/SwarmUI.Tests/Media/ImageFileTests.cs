using Xunit;
using SwarmUI.Media;
using System;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void FromDataString_InvalidInput_ThrowsException()
        {
            // Test with a completely invalid string
            Assert.Throws<FormatException>(() => ImageFile.FromDataString("not a data string"));

            // Test with invalid base64 content
            Assert.Throws<FormatException>(() => ImageFile.FromDataString("data:image/png;base64,not-valid-base64"));

            // Test with missing comma
            Assert.Throws<FormatException>(() => ImageFile.FromDataString("data:image/png;base64"));

            // Test with null input
            Assert.Throws<NullReferenceException>(() => ImageFile.FromDataString(null));
        }
    }
}
