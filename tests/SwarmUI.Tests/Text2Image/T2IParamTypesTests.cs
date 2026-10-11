using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Theory]
        [InlineData("ValidName", "validname")]
        [InlineData("  Spaces  ", "spaces")]
        [InlineData("With Space", "withspace")]
        [InlineData("symbols-and_stuff!", "symbolsandstuff")]
        [InlineData("123Numbers", "numbers")]
        [InlineData("", "")]
        [InlineData("!@#$%", "")]
        [InlineData("  multiple   spaces  ", "multiplespaces")]
        [InlineData("camelCaseParam", "camelcaseparam")]
        public void CleanTypeName_ReturnsExpectedString(string input, string expected)
        {
            string result = T2IParamTypes.CleanTypeName(input);
            Assert.Equal(expected, result);
        }
    }
}
