using Xunit;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.Text2Image
{
    public class T2IParamTypesTests
    {
        [Fact]
        public void ApplyStringEdit_NoValuePlaceholder_ReturnsUpdate()
        {
            string result = T2IParamTypes.ApplyStringEdit("prior string", "update string");
            Assert.Equal("update string", result);
        }

        [Fact]
        public void ApplyStringEdit_UpdateContainsValueAndPriorIsNull_ReplacesWithEmptyString()
        {
            string result = T2IParamTypes.ApplyStringEdit(null, "new {value} update");
            Assert.Equal("new  update", result);
        }

        [Fact]
        public void ApplyStringEdit_UpdateContainsValueAndPriorHasNoTags_ReplacesValue()
        {
            string result = T2IParamTypes.ApplyStringEdit("prior string", "new {value} update");
            Assert.Equal("new prior string update", result);
        }

        [Theory]
        [InlineData("<segment:123>")]
        [InlineData("<object:123>")]
        [InlineData("<region:123>")]
        public void ApplyStringEdit_UpdateContainsValueAndPriorHasTags_ReplacesValueAndAppendsTags(string tag)
        {
            string prior = $"a beautiful landscape {tag}";
            string update = "very {value}";
            string expected = $"very a beautiful landscape {tag}";

            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ApplyStringEdit_PriorHasMultipleTags_FindsFirstTag()
        {
            string prior = "a beautiful landscape <object:1> <segment:2>";
            string update = "very {value}";
            string expected = "very a beautiful landscape <object:1> <segment:2>";

            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ApplyStringEdit_PriorHasUppercaseTags_FindsTagSuccessfully()
        {
            string prior = "a beautiful landscape <REGION:123>";
            string update = "very {value}";
            string expected = "very a beautiful landscape <REGION:123>";

            string result = T2IParamTypes.ApplyStringEdit(prior, update);
            Assert.Equal(expected, result);
        }
    }
}
