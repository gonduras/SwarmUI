using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class MediaTypeTests
    {
        [Fact]
        public void GetByExtension_RegisteredExtension_ReturnsCorrectMediaType()
        {
            // Arrange
            // Act
            var type = MediaType.GetByExtension("png");

            // Assert
            Assert.NotNull(type);
            Assert.Equal("png", type.Extension);
            Assert.Equal("image/png", type.MimeType);
            Assert.Equal(MediaMetaType.Image, type.MetaType);
        }

        [Fact]
        public void GetByExtension_AltExtension_ReturnsCorrectMediaType()
        {
            // Arrange
            // Act
            var type = MediaType.GetByExtension("jpeg");

            // Assert
            Assert.NotNull(type);
            Assert.Equal("jpg", type.Extension);
            Assert.Equal("image/jpeg", type.MimeType);
            Assert.Equal(MediaMetaType.Image, type.MetaType);
        }

        [Fact]
        public void GetByExtension_UnknownExtensionWithPresumptions_ReturnsNewMediaType()
        {
            // Arrange
            string extension = "foo";
            string mimePresumption = "custom";
            MediaMetaType defaultPresumption = MediaMetaType.Text;

            // Act
            var type = MediaType.GetByExtension(extension, mimePresumption, defaultPresumption);

            // Assert
            Assert.NotNull(type);
            Assert.Equal(extension, type.Extension);
            Assert.Equal($"{mimePresumption}/{extension}", type.MimeType);
            Assert.Equal(defaultPresumption, type.MetaType);

            // Note: Does not register the new type automatically
            var checkAgain = MediaType.GetByExtension(extension);
            Assert.Null(checkAgain);
        }

        [Fact]
        public void GetByExtension_UnknownExtensionNoPresumptions_ReturnsNull()
        {
            // Arrange
            string extension = "unknown";

            // Act
            var type = MediaType.GetByExtension(extension);

            // Assert
            Assert.Null(type);
        }

        [Fact]
        public void GetByExtension_UnknownExtensionPartialPresumptions_ReturnsNull()
        {
            // Arrange
            string extension = "unknown";

            // Act
            var type1 = MediaType.GetByExtension(extension, mimePresumption: "custom");
            var type2 = MediaType.GetByExtension(extension, defaultPresumption: MediaMetaType.Text);

            // Assert
            Assert.Null(type1);
            Assert.Null(type2);
        }
    }
}
