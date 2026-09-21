using Xunit;
using SwarmUI.Media;
using SixLabors.ImageSharp.Formats.Png;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void FastPngEncoder_HasCorrectConfiguration()
        {
            var encoder = ImageFile.FastPngEncoder;

            // Check it's not null
            Assert.NotNull(encoder);

            // Fast png encoding is meant to be fast, so level 1 is expected
            Assert.Equal(PngCompressionLevel.Level1, encoder.CompressionLevel);
        }
    }
}
