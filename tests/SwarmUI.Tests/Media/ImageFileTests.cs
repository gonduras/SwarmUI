using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Theory]
        [InlineData("PNG", "png")]
        [InlineData("JPG", "jpg")]
        [InlineData("JPG90", "jpg")]
        [InlineData("JPG75", "jpg")]
        [InlineData("WEBP_LOSSLESS", "webp")]
        [InlineData("WEBP_100", "webp")]
        [InlineData("WEBP_90", "webp")]
        [InlineData("WEBP_75", "webp")]
        [InlineData("WEBP", "webp")]
        public void ImageFormatToExtension_ValidFormats_ReturnsExpectedExtension(string format, string expectedExtension)
        {
            // Act
            string extension = ImageFile.ImageFormatToExtension(format);

            // Assert
            Assert.Equal(expectedExtension, extension);
        }

        [Fact]
        public void ImageFormatToExtension_InvalidFormat_ThrowsArgumentException()
        {
            // Arrange
            string invalidFormat = "INVALID_FORMAT";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => ImageFile.ImageFormatToExtension(invalidFormat));
            Assert.Contains("Unknown format: " + invalidFormat, ex.Message);
            Assert.Equal("format", ex.ParamName);
        }
    }
}
