using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Theory]
        [InlineData("Model", "model")]
        [InlineData(" Model ", "model")]
        [InlineData("Model Name", "modelname")]
        [InlineData("Model_Name", "modelname")]
        [InlineData("123Model", "model")]
        [InlineData("Model123", "model")]
        [InlineData("123Model123", "model")]
        [InlineData("  !@#Model!@#  ", "model")]
        public void CleanTypeName_ReturnsCleanedString(string input, string expected)
        {
            // Act
            string result = T2IParamTypes.CleanTypeName(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
