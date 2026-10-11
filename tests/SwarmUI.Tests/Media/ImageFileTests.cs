using System;
using Xunit;
using SwarmUI.Media;
using System.Linq;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void FromDataString_ValidImage_ReturnsImageFile()
        {
            // Arrange
            // 1x1 transparent png
            string base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
            string dataString = $"data:image/png;base64,{base64Data}";

            // Act
            ImageFile result = ImageFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MediaType.ImagePng, result.Type);
            Assert.Equal(MediaMetaType.Image, result.Type.MetaType);
            byte[] expectedRaw = Convert.FromBase64String(base64Data);
            Assert.True(result.RawData.SequenceEqual(expectedRaw));
        }

        [Fact]
        public void FromDataString_ValidVideo_ReturnsVideoFile()
        {
            // Arrange
            string base64Data = "AAAAHGZ0eXBtcDQyAAAAAW1wNDFpc29tAA==";
            string dataString = $"data:video/mp4;base64,{base64Data}";

            // Act
            ImageFile result = ImageFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("video/mp4", result.Type.MimeType);
            Assert.Equal(MediaMetaType.Video, result.Type.MetaType);
            byte[] expectedRaw = Convert.FromBase64String(base64Data);
            Assert.True(result.RawData.SequenceEqual(expectedRaw));
        }

        [Fact]
        public void FromDataString_UnknownMimeType_FallsBackGracefully()
        {
            // Arrange
            string base64Data = "AQIDBA==";
            string dataString = $"data:image/unknown_xyz;base64,{base64Data}";

            // Act
            ImageFile result = ImageFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("image/unknown_xyz", result.Type.MimeType);
            Assert.Equal("unknown_xyz", result.Type.Extension);
            Assert.Equal(MediaMetaType.Image, result.Type.MetaType);
            byte[] expectedRaw = Convert.FromBase64String(base64Data);
            Assert.True(result.RawData.SequenceEqual(expectedRaw));
        }

        [Fact]
        public void FromDataString_UnknownVideoType_FallsBackGracefullyToVideoMetaType()
        {
            // Arrange
            string base64Data = "AQIDBA==";
            string dataString = $"data:video/unknown_xyz;base64,{base64Data}";

            // Act
            ImageFile result = ImageFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("video/unknown_xyz", result.Type.MimeType);
            Assert.Equal("unknown_xyz", result.Type.Extension);
            Assert.Equal(MediaMetaType.Video, result.Type.MetaType);
            byte[] expectedRaw = Convert.FromBase64String(base64Data);
            Assert.True(result.RawData.SequenceEqual(expectedRaw));
        }

        [Fact]
        public void FromDataString_InvalidBase64_ThrowsFormatException()
        {
            // Arrange
            string dataString = "data:image/png;base64,invalid_base64!";

            // Act & Assert
            Assert.Throws<FormatException>(() => ImageFile.FromDataString(dataString));
        }

        [Fact]
        public void FromDataString_NullData_ThrowsNullReferenceException()
        {
            // Arrange
            string dataString = null;

            // Act & Assert
            // FreneticExtensions string.After() on null throws NullReferenceException
            Assert.Throws<NullReferenceException>(() => ImageFile.FromDataString(dataString));
        }
    }
}
