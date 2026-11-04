using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class VideoFileTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            byte[] raw = new byte[] { 1, 2, 3 };
            var type = MediaType.VideoMp4;

            var video = new VideoFile(raw, type);

            Assert.Same(raw, video.RawData);
            Assert.Same(type, video.Type);
        }

        [Fact]
        public void FromBase64_ParsesCorrectly()
        {
            byte[] raw = new byte[] { 1, 2, 3, 4 };
            string b64 = Convert.ToBase64String(raw);
            var type = MediaType.VideoMp4;

            var video = VideoFile.FromBase64(b64, type);

            Assert.Equal(raw, video.RawData);
            Assert.Same(type, video.Type);
        }

        [Fact]
        public void FromBase64_InvalidBase64_ThrowsException()
        {
            Assert.Throws<FormatException>(() => VideoFile.FromBase64("invalid_base64", MediaType.VideoMp4));
        }

        [Fact]
        public void FromDataString_ParsesCorrectly_KnownMimeType()
        {
            byte[] raw = new byte[] { 1, 2, 3, 4 };
            string b64 = Convert.ToBase64String(raw);
            string dataStr = $"data:video/mp4;base64,{b64}";

            var video = VideoFile.FromDataString(dataStr);

            Assert.Equal(raw, video.RawData);
            Assert.Same(MediaType.VideoMp4, video.Type);
        }

        [Fact]
        public void FromDataString_ParsesCorrectly_UnknownMimeType()
        {
            byte[] raw = new byte[] { 1, 2, 3, 4 };
            string b64 = Convert.ToBase64String(raw);
            string dataStr = $"data:video/unknown;base64,{b64}";

            var video = VideoFile.FromDataString(dataStr);

            Assert.Equal(raw, video.RawData);
            Assert.Equal("video/unknown", video.Type.MimeType);
            Assert.Equal("unknown", video.Type.Extension);
            Assert.Equal(MediaMetaType.Video, video.Type.MetaType);
        }

        [Fact]
        public void FromDataString_InvalidDataString_ThrowsException()
        {
            Assert.Throws<FormatException>(() => VideoFile.FromDataString("invalid_data_string_no_comma"));
        }
    }
}
