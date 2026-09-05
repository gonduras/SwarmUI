using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwarmUI.WebAPI;
using SwarmUI.Accounts;
using SwarmUI.Utils;
using Xunit;

namespace SwarmUI.Tests
{
    public class T2IAPITests
    {
        [Fact]
        public void ProcessGenerateText2ImageResults_HappyPath_ReturnsImages()
        {
            // Arrange
            var outputs = new List<JObject>
            {
                new JObject
                {
                    ["image"] = "path/to/image1.png",
                    ["batch_index"] = 0
                },
                new JObject
                {
                    ["image"] = "path/to/image2.png",
                    ["batch_index"] = 1
                }
            };

            // Act
            var result = T2IAPI.ProcessGenerateText2ImageResults(outputs);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("images"));
            var images = result["images"] as JArray;
            Assert.NotNull(images);
            Assert.Equal(2, images.Count);
            Assert.Equal("path/to/image1.png", images[0].ToString());
            Assert.Equal("path/to/image2.png", images[1].ToString());
        }

        [Fact]
        public void ProcessGenerateText2ImageResults_WithError_ReturnsError()
        {
            // Arrange
            var outputs = new List<JObject>
            {
                new JObject
                {
                    ["image"] = "path/to/image1.png",
                    ["batch_index"] = 0
                },
                new JObject
                {
                    ["error"] = "Something went wrong"
                },
                new JObject
                {
                    ["image"] = "path/to/image2.png",
                    ["batch_index"] = 1
                }
            };

            // Act
            var result = T2IAPI.ProcessGenerateText2ImageResults(outputs);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("error"));
            Assert.Equal("Something went wrong", result["error"]?.ToString());
            Assert.False(result.ContainsKey("images")); // Should not have images if error returned early
        }

        [Fact]
        public void ProcessGenerateText2ImageResults_WithDiscardIndices_ReturnsRemainingImages()
        {
            // Arrange
            var outputs = new List<JObject>
            {
                new JObject
                {
                    ["image"] = "path/to/image1.png",
                    ["batch_index"] = 0
                },
                new JObject
                {
                    ["image"] = "path/to/image2.png",
                    ["batch_index"] = 1
                },
                new JObject
                {
                    ["image"] = "path/to/image3.png",
                    ["batch_index"] = 2
                },
                new JObject
                {
                    ["discard_indices"] = new JArray(1) // Discard the image at batch_index 1
                }
            };

            // Act
            var result = T2IAPI.ProcessGenerateText2ImageResults(outputs);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("images"));
            var images = result["images"] as JArray;
            Assert.NotNull(images);
            Assert.Equal(2, images.Count);
            Assert.Equal("path/to/image1.png", images[0].ToString());
            Assert.Equal("path/to/image3.png", images[1].ToString()); // index 1 (image2) was discarded
        }

        [Fact]
        public void ProcessGenerateText2ImageResults_WithMultipleDiscardIndices_ReturnsRemainingImages()
        {
            // Arrange
            var outputs = new List<JObject>
            {
                new JObject
                {
                    ["image"] = "path/to/image1.png",
                    ["batch_index"] = 0
                },
                new JObject
                {
                    ["image"] = "path/to/image2.png",
                    ["batch_index"] = 1
                },
                new JObject
                {
                    ["image"] = "path/to/image3.png",
                    ["batch_index"] = 2
                },
                new JObject
                {
                    ["discard_indices"] = new JArray(0, 2) // Discard the image at batch_index 0 and 2
                }
            };

            // Act
            var result = T2IAPI.ProcessGenerateText2ImageResults(outputs);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("images"));
            var images = result["images"] as JArray;
            Assert.NotNull(images);
            Assert.Single(images);
            Assert.Equal("path/to/image2.png", images[0].ToString()); // index 0 and 2 were discarded
        }
    }
}
