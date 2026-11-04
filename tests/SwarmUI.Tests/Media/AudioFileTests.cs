using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class AudioFileTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            byte[] data = { 1, 2, 3 };
            MediaType type = MediaType.AudioMp3;

            // Act
            AudioFile audioFile = new AudioFile(data, type);

            // Assert
            Assert.Equal(data, audioFile.RawData);
            Assert.Equal(type, audioFile.Type);
        }

        [Fact]
        public void FromBase64_CreatesCorrectAudioFile()
        {
            // Arrange
            string base64 = "AQID"; // [1, 2, 3] in base64
            MediaType type = MediaType.AudioWav;

            // Act
            AudioFile audioFile = AudioFile.FromBase64(base64, type);

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3 }, audioFile.RawData);
            Assert.Equal(type, audioFile.Type);
        }

        [Fact]
        public void FromBase64_InvalidBase64_ThrowsFormatException()
        {
            // Arrange
            string invalidBase64 = "not-base64!";
            MediaType type = MediaType.AudioWav;

            // Act & Assert
            Assert.Throws<FormatException>(() => AudioFile.FromBase64(invalidBase64, type));
        }

        [Fact]
        public void FromDataString_CreatesCorrectAudioFile_KnownType()
        {
            // Arrange
            string dataString = "data:audio/mpeg;base64,AQID"; // [1, 2, 3] in base64

            // Act
            AudioFile audioFile = AudioFile.FromDataString(dataString);

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3 }, audioFile.RawData);
            Assert.Equal(MediaType.AudioMp3, audioFile.Type);
        }

        [Fact]
        public void FromDataString_CreatesCorrectAudioFile_UnknownType()
        {
            // Arrange
            string dataString = "data:audio/custom-audio;base64,AQID"; // [1, 2, 3] in base64

            // Act
            AudioFile audioFile = AudioFile.FromDataString(dataString);

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3 }, audioFile.RawData);
            Assert.NotNull(audioFile.Type);
            Assert.Equal("custom-audio", audioFile.Type.Extension);
            Assert.Equal("audio/custom-audio", audioFile.Type.MimeType);
            Assert.Equal(MediaMetaType.Audio, audioFile.Type.MetaType);
        }

        [Fact]
        public void FromDataString_InvalidBase64_ThrowsFormatException()
        {
            // Arrange
            string dataString = "data:audio/mpeg;base64,invalid-base64";

            // Act & Assert
            Assert.Throws<FormatException>(() => AudioFile.FromDataString(dataString));
        }

        [Fact]
        public void FromDataString_InvalidFormat_ThrowsException()
        {
            // Arrange
            string dataString = "invalid-data-string";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => AudioFile.FromDataString(dataString));
        }
    }
}
