using System;
using Xunit;
using SwarmUI.Media;

namespace SwarmUI.Tests.Media
{
    public class MediaTypeTests
    {
        [Fact]
        public void GetByExtension_ExistingExtension_ReturnsMediaType()
        {
            // Arrange
            string extension = "png";

            // Act
            var result = MediaType.GetByExtension(extension);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MediaType.ImagePng, result);
        }

        [Fact]
        public void GetByExtension_NonExistingExtensionWithPresumptions_ReturnsNewMediaType()
        {
            // Arrange
            string extension = "unknown_ext";
            string mimePresumption = "image";
            MediaMetaType defaultPresumption = MediaMetaType.Image;

            // Act
            var result = MediaType.GetByExtension(extension, mimePresumption, defaultPresumption);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(extension, result.Extension);
            Assert.Equal($"{mimePresumption}/{extension}", result.MimeType);
            Assert.Equal(defaultPresumption, result.MetaType);
        }

        [Fact]
        public void GetByExtension_NonExistingExtensionNullMimePresumption_ReturnsNull()
        {
            // Arrange
            string extension = "unknown_ext2";
            string mimePresumption = null;
            MediaMetaType defaultPresumption = MediaMetaType.Image;

            // Act
            var result = MediaType.GetByExtension(extension, mimePresumption, defaultPresumption);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByExtension_NonExistingExtensionNullDefaultPresumption_ReturnsNull()
        {
            // Arrange
            string extension = "unknown_ext3";
            string mimePresumption = "image";
            MediaMetaType defaultPresumption = null;

            // Act
            var result = MediaType.GetByExtension(extension, mimePresumption, defaultPresumption);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByExtension_NonExistingExtensionBothPresumptionsNull_ReturnsNull()
        {
            // Arrange
            string extension = "unknown_ext4";
            string mimePresumption = null;
            MediaMetaType defaultPresumption = null;

            // Act
            var result = MediaType.GetByExtension(extension, mimePresumption, defaultPresumption);

            // Assert
            Assert.Null(result);
        }
    }
}
