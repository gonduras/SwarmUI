using System;
using Xunit;
using SwarmUI.Media;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void FromDataString_ValidImagePng_ReturnsImageWithCorrectDataAndType()
        {
            string data = "data:image/png;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);

            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.Equal(MediaType.ImagePng, result.Type);
        }

        [Fact]
        public void FromDataString_UnknownImageMimeType_ReturnsImageWithFallbackMediaType()
        {
            string data = "data:image/unknown;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);

            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.NotNull(result.Type);
            Assert.Equal("image/unknown", result.Type.MimeType);
            Assert.Equal("unknown", result.Type.Extension);
            Assert.Equal(MediaMetaType.Image, result.Type.MetaType);
        }

        [Fact]
        public void FromDataString_UnknownVideoMimeType_ReturnsImageWithVideoMetaType()
        {
            string data = "data:video/unknown;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);

            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.NotNull(result.Type);
            Assert.Equal("video/unknown", result.Type.MimeType);
            Assert.Equal("unknown", result.Type.Extension);
            Assert.Equal(MediaMetaType.Video, result.Type.MetaType);
        }

        [Fact]
        public void FromDataString_InvalidBase64_ThrowsFormatException()
        {
            string data = "data:image/png;base64,not-valid-base64!";
            Assert.Throws<FormatException>(() => ImageFile.FromDataString(data));
        }

        [Fact]
        public void FromDataString_NullInput_ThrowsNullReferenceException()
        {
            Assert.Throws<NullReferenceException>(() => ImageFile.FromDataString(null));
        }
    }
}
