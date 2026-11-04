using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class VideoFileTests
    {
        [Fact]
        public void FromDataString_ValidKnownMimeType_ReturnsVideoFile()
        {
            // Arrange
            string dataString = "data:video/mp4;base64,AQIDBA==";

            // Act
            VideoFile videoFile = VideoFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(videoFile);
            Assert.Equal(MediaType.VideoMp4, videoFile.Type);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, videoFile.RawData);
        }

        [Fact]
        public void FromDataString_UnknownMimeType_CreatesFallbackMediaType()
        {
            // Arrange
            string dataString = "data:video/unknown;base64,AQIDBA==";

            // Act
            VideoFile videoFile = VideoFile.FromDataString(dataString);

            // Assert
            Assert.NotNull(videoFile);
            Assert.NotNull(videoFile.Type);
            Assert.Equal("video/unknown", videoFile.Type.MimeType);
            Assert.Equal("unknown", videoFile.Type.Extension);
            Assert.Equal(MediaMetaType.Video, videoFile.Type.MetaType);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, videoFile.RawData);
        }

        [Fact]
        public void FromDataString_InvalidBase64_ThrowsException()
        {
            // Arrange
            string dataString = "data:video/mp4;base64,invalid_base64!!!";

            // Act & Assert
            Assert.Throws<FormatException>(() => VideoFile.FromDataString(dataString));
        }


        [Fact]
        public void FromDataString_NoComma_ThrowsArgumentException()
        {
            // Arrange
            string dataString = "invalid_data_string_without_comma";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => VideoFile.FromDataString(dataString));
        }

        [Fact]
        public void FromBase64_ValidBase64_ReturnsVideoFile()
        {
            // Arrange
            string b64 = "AQIDBA==";
            MediaType type = MediaType.VideoWebm;

            // Act
            VideoFile videoFile = VideoFile.FromBase64(b64, type);

            // Assert
            Assert.NotNull(videoFile);
            Assert.Equal(type, videoFile.Type);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, videoFile.RawData);
        }

        [Fact]
        public void FromBase64_InvalidBase64_ThrowsException()
        {
            // Arrange
            string b64 = "invalid_base64!!!";
            MediaType type = MediaType.VideoWebm;

            // Act & Assert
            Assert.Throws<FormatException>(() => VideoFile.FromBase64(b64, type));
        }

        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            byte[] data = new byte[] { 5, 6, 7, 8 };
            MediaType type = MediaType.VideoMov;

            // Act
            VideoFile videoFile = new VideoFile(data, type);

            // Assert
            Assert.Equal(data, videoFile.RawData);
            Assert.Equal(type, videoFile.Type);
        }
    }
}
