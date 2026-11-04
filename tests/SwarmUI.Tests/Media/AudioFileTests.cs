using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class AudioFileTests
    {
        [Fact]
        public void FromDataString_ParsesValidDataString()
        {
            // Arrange
            // 'test' base64 is 'dGVzdA=='
            string data = "data:audio/wav;base64,dGVzdA==";

            // Act
            AudioFile audio = AudioFile.FromDataString(data);

            // Assert
            Assert.NotNull(audio);
            Assert.Equal(new byte[] { 116, 101, 115, 116 }, audio.RawData); // UTF-8 bytes for 'test'
            Assert.Equal("wav", audio.Type.Extension);
            Assert.Equal("audio/wav", audio.Type.MimeType);
            Assert.Equal(MediaMetaType.Audio, audio.Type.MetaType);
        }

        [Fact]
        public void FromDataString_CreatesNewTypeForUnknownMimeType()
        {
            // Arrange
            string data = "data:audio/unknown;base64,dGVzdA==";

            // Act
            AudioFile audio = AudioFile.FromDataString(data);

            // Assert
            Assert.NotNull(audio);
            Assert.Equal(new byte[] { 116, 101, 115, 116 }, audio.RawData);
            Assert.Equal("unknown", audio.Type.Extension);
            Assert.Equal("audio/unknown", audio.Type.MimeType);
            Assert.Equal(MediaMetaType.Audio, audio.Type.MetaType);
        }

        [Fact]
        public void FromDataString_InvalidBase64ThrowsException()
        {
            // Arrange
            string data = "data:audio/wav;base64,invalid-base64!";

            // Act & Assert
            Assert.Throws<FormatException>(() => AudioFile.FromDataString(data));
        }

        [Fact]
        public void FromDataString_MissingCommaThrowsException()
        {
            // Arrange
            string data = "data:audio/wav;base64dGVzdA==";

            // Act & Assert
            // When string lacks a comma, it should throw InvalidOperationException because the extension method Before()/After() does not find the character
            // Or if they fix the parsing logic, it will throw ArgumentException
            Assert.ThrowsAny<Exception>(() => AudioFile.FromDataString(data));
        }

        [Fact]
        public void FromBase64_ParsesValidBase64()
        {
            // Arrange
            string base64 = "dGVzdA==";
            MediaType type = MediaType.AudioWav;

            // Act
            AudioFile audio = AudioFile.FromBase64(base64, type);

            // Assert
            Assert.NotNull(audio);
            Assert.Equal(new byte[] { 116, 101, 115, 116 }, audio.RawData);
            Assert.Equal(type, audio.Type);
        }

        [Fact]
        public void FromBase64_InvalidBase64ThrowsException()
        {
            // Arrange
            string base64 = "invalid-base64!";
            MediaType type = MediaType.AudioWav;

            // Act & Assert
            Assert.Throws<FormatException>(() => AudioFile.FromBase64(base64, type));
        }

        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            byte[] data = new byte[] { 1, 2, 3 };
            MediaType type = MediaType.AudioWav;

            // Act
            AudioFile audio = new AudioFile(data, type);

            // Assert
            Assert.Equal(data, audio.RawData);
            Assert.Equal(type, audio.Type);
        }
    }
}
