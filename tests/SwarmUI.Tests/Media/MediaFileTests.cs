using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class MediaFileTests
    {
        [Fact]
        public void AsBase64_NormalBytes_ReturnsCorrectBase64()
        {
            // Arrange
            byte[] rawData = { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
            string expectedBase64 = Convert.ToBase64String(rawData); // "SGVsbG8="
            var mediaFile = new MediaFile
            {
                RawData = rawData,
                Type = new MediaType("png", "image/png", null)
            };

            // Act
            string actualBase64 = mediaFile.AsBase64;

            // Assert
            Assert.Equal(expectedBase64, actualBase64);
        }

        [Fact]
        public void AsBase64_EmptyBytes_ReturnsEmptyString()
        {
            // Arrange
            byte[] rawData = Array.Empty<byte>();
            string expectedBase64 = string.Empty;
            var mediaFile = new MediaFile
            {
                RawData = rawData,
                Type = new MediaType("png", "image/png", null)
            };

            // Act
            string actualBase64 = mediaFile.AsBase64;

            // Assert
            Assert.Equal(expectedBase64, actualBase64);
        }

        [Fact]
        public void AsDataString_NormalBytes_ReturnsCorrectDataString()
        {
            // Arrange
            byte[] rawData = { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
            string base64 = Convert.ToBase64String(rawData); // "SGVsbG8="
            string mimeType = "image/png";
            string expectedDataString = $"data:{mimeType};base64,{base64}";
            var mediaFile = new MediaFile
            {
                RawData = rawData,
                Type = new MediaType("png", mimeType, null)
            };

            // Act
            string actualDataString = mediaFile.AsDataString();

            // Assert
            Assert.Equal(expectedDataString, actualDataString);
        }

        [Fact]
        public void AsDataString_EmptyBytes_ReturnsCorrectDataString()
        {
            // Arrange
            byte[] rawData = Array.Empty<byte>();
            string base64 = string.Empty;
            string mimeType = "image/png";
            string expectedDataString = $"data:{mimeType};base64,{base64}";
            var mediaFile = new MediaFile
            {
                RawData = rawData,
                Type = new MediaType("png", mimeType, null)
            };

            // Act
            string actualDataString = mediaFile.AsDataString();

            // Assert
            Assert.Equal(expectedDataString, actualDataString);
        }
    }
}
