using System;
using Xunit;
using SwarmUI.Media;
using FreneticUtilities.FreneticExtensions;

namespace SwarmUI.Tests.Media
{
    public class ImageFileTests
    {
        [Fact]
        public void FromDataString_ValidImagePng_ReturnsCorrectImageFile()
        {
            string data = "data:image/png;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);
            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.Equal(MediaType.ImagePng, result.Type);
        }

        [Fact]
        public void FromDataString_ValidImageJpeg_ReturnsCorrectImageFile()
        {
            string data = "data:image/jpeg;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);
            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.Equal(MediaType.ImageJpg, result.Type);
        }

        [Fact]
        public void FromDataString_UnknownMimeType_CreatesNewMediaType()
        {
            string data = "data:image/unknown;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);
            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.Equal("image/unknown", result.Type.MimeType);
            Assert.Equal("unknown", result.Type.Extension);
            Assert.Equal(MediaMetaType.Image, result.Type.MetaType);
        }

        [Fact]
        public void FromDataString_UnknownVideoType_CreatesNewMediaType()
        {
            string data = "data:video/unknown;base64,AQIDBA==";
            ImageFile result = ImageFile.FromDataString(data);
            Assert.NotNull(result);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.RawData);
            Assert.Equal("video/unknown", result.Type.MimeType);
            Assert.Equal("unknown", result.Type.Extension);
            Assert.Equal(MediaMetaType.Video, result.Type.MetaType);
        }

        [Fact]
        public void FromDataString_InvalidBase64_ThrowsFormatException()
        {
            string data = "data:image/png;base64,invalid_base64!!!";
            Assert.Throws<FormatException>(() => ImageFile.FromDataString(data));
        }

        [Fact]
        public void FromDataString_InvalidStringMissingBase64_ThrowsFormatException()
        {
            string data = "invalid_string";
            Assert.Throws<FormatException>(() => ImageFile.FromDataString(data));
        }

        [Fact]
        public void FromDataString_EmptyString_ReturnsEmptyRawData()
        {
            string data = "";
            ImageFile result = ImageFile.FromDataString(data);
            Assert.NotNull(result);
            Assert.Empty(result.RawData);
        }

        [Fact]
        public void FromDataString_NullString_ThrowsNullReferenceException()
        {
#pragma warning disable CS8600
            string data = null;
#pragma warning restore CS8600
#pragma warning disable CS8604
            Assert.Throws<NullReferenceException>(() => ImageFile.FromDataString(data));
#pragma warning restore CS8604
        }
    }
}
