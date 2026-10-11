using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Fact]
        public void ApplyStringEdit_NoValueTag_ReturnsUpdate()
        {
            string prior = "original text";
            string update = "new text without tag";
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal("new text without tag", result);
        }

        [Fact]
        public void ApplyStringEdit_PriorIsNull_ReplacesValueWithEmptyString()
        {
            string prior = null;
            string update = "prefix {value} suffix";
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal("prefix  suffix", result);
        }

        [Fact]
        public void ApplyStringEdit_NormalPrior_ReplacesValue()
        {
            string prior = "original";
            string update = "prefix {value} suffix";
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal("prefix original suffix", result);
        }

        [Theory]
        [InlineData("base text <segment:xyz> extra", "new {value} text", "new base text text <segment:xyz> extra")]
        [InlineData("base text <object:abc> extra", "new {value} text", "new base text text <object:abc> extra")]
        [InlineData("base text <region:123> extra", "new {value} text", "new base text text <region:123> extra")]
        public void ApplyStringEdit_PriorHasTags_SplitsCorrectly(string prior, string update, string expected)
        {
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ApplyStringEdit_PriorHasMultipleTags_SplitsAtEarliest()
        {
            string prior = "base <object:abc> <segment:xyz>";
            string update = "{value} appended";
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal("base appended <object:abc> <segment:xyz>", result);
        }

        [Fact]
        public void ApplyStringEdit_PriorIsOnlyTag_EmptyValue()
        {
            string prior = "<segment:xyz>";
            string update = "prefix {value}";
            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal("prefix  <segment:xyz>", result);
        }
    }
}
