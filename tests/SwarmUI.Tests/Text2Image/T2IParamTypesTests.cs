using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Theory]
        [InlineData("1:1", 512, 512)]
        [InlineData("4:3", 576, 448)]
        [InlineData("3:2", 608, 416)]
        [InlineData("8:5", 608, 384)]
        [InlineData("16:9", 672, 384)]
        [InlineData("21:9", 768, 320)]
        [InlineData("3:4", 448, 576)]
        [InlineData("2:3", 416, 608)]
        [InlineData("5:8", 384, 608)]
        [InlineData("9:16", 384, 672)]
        [InlineData("9:21", 320, 768)]
        public void AspectRatioToSizeReference_ValidAspectRatios_ReturnsCorrectSize(string aspectRatio, int expectedWidth, int expectedHeight)
        {
            // Act
            var (width, height) = T2IParamTypes.AspectRatioToSizeReference(aspectRatio);

            // Assert
            Assert.Equal(expectedWidth, width);
            Assert.Equal(expectedHeight, height);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid_format")]
        [InlineData("1")]
        [InlineData("1:1:1")]
        public void AspectRatioToSizeReference_InvalidAspectRatios_ReturnsNegativeOne(string? aspectRatio)
        {
            // Act
            var (width, height) = T2IParamTypes.AspectRatioToSizeReference(aspectRatio);

            // Assert
            Assert.Equal(-1, width);
            Assert.Equal(-1, height);
        }
    }
}
