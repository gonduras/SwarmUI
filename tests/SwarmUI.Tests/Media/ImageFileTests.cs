using System;
using Xunit;
using SwarmUI.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void ISImgToPngBytes_ReturnsValidPngBytes()
        {
            // Arrange
            using var image = new Image<Rgba32>(10, 10);

            // Act
            byte[] result = ImageFile.ISImgToPngBytes(image);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // PNG magic number validation: 89 50 4E 47 0D 0A 1A 0A
            Assert.True(result.Length > 8);
            Assert.Equal(0x89, result[0]);
            Assert.Equal(0x50, result[1]); // P
            Assert.Equal(0x4E, result[2]); // N
            Assert.Equal(0x47, result[3]); // G
        }

        [Fact]
        public void ISImgToJpgBytes_ReturnsValidJpgBytes()
        {
            // Arrange
            using var image = new Image<Rgba32>(10, 10);

            // Act
            byte[] result = ImageFile.ISImgToJpgBytes(image);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // JPEG magic number validation: FF D8 FF
            Assert.True(result.Length > 3);
            Assert.Equal(0xFF, result[0]);
            Assert.Equal(0xD8, result[1]);
            Assert.Equal(0xFF, result[2]);
        }
    }
}
