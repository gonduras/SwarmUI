using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Theory]
        [InlineData("validname", "validname")]
        [InlineData("ValidName", "validname")]
        [InlineData("  valid name  ", "validname")]
        [InlineData("Valid Name", "validname")]
        [InlineData("valid_name", "validname")]
        [InlineData("valid-name", "validname")]
        [InlineData("valid123name", "validname")]
        [InlineData("!@#valid$%^name&*()", "validname")]
        public void CleanTypeName_ReturnsExpectedResult(string input, string expected)
        {
            // Act
            string result = T2IParamTypes.CleanTypeName(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
