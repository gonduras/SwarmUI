using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class MediaFileTests
    {
        [Fact]
        public void AsBase64_ConvertsRawDataToBase64()
        {
            // Arrange
            var mediaFile = new MediaFile
            {
                RawData = new byte[] { 1, 2, 3, 4 }
            };

            // Act
            string base64 = mediaFile.AsBase64;

            // Assert
            Assert.Equal("AQIDBA==", base64);
        }

        [Fact]
        public void AsDataString_ReturnsValidDataString()
        {
            // Arrange
            var mediaFile = new MediaFile
            {
                RawData = new byte[] { 1, 2, 3, 4 },
                Type = MediaType.ImagePng
            };

            // Act
            string dataString = mediaFile.AsDataString();

            // Assert
            Assert.Equal("data:image/png;base64,AQIDBA==", dataString);
        }

        [Fact]
        public void EmptyRawData_ReturnsEmptyBase64AndValidDataString()
        {
            // Arrange
            var mediaFile = new MediaFile
            {
                RawData = Array.Empty<byte>(),
                Type = MediaType.ImagePng
            };

            // Act
            string base64 = mediaFile.AsBase64;
            string dataString = mediaFile.AsDataString();

            // Assert
            Assert.Equal("", base64);
            Assert.Equal("data:image/png;base64,", dataString);
        }

        [Fact]
        public void NullRawData_ThrowsException()
        {
            // Arrange
            var mediaFile = new MediaFile
            {
                RawData = null,
                Type = MediaType.ImagePng
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mediaFile.AsBase64);
            Assert.Throws<ArgumentNullException>(() => mediaFile.AsDataString());
        }
    }
}
