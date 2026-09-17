using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class WebUtilTests
    {
        [Fact]
        public void JSStringEscape_EmptyString_ReturnsEmptyString()
        {
            var result = WebUtil.JSStringEscape("");
            Assert.Equal("", result);
        }

        [Fact]
        public void JSStringEscape_NormalString_ReturnsSameString()
        {
            var result = WebUtil.JSStringEscape("hello world");
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void JSStringEscape_DoubleQuotes_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("\"hello\"");
            Assert.Equal("\\\"hello\\\"", result);
        }

        [Fact]
        public void JSStringEscape_SingleQuotes_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("'hello'");
            Assert.Equal("\\'hello\\'", result);
        }

        [Fact]
        public void JSStringEscape_Newlines_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("hello\nworld");
            Assert.Equal("hello\\nworld", result);
        }

        [Fact]
        public void JSStringEscape_CarriageReturns_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("hello\rworld");
            Assert.Equal("hello\\rworld", result);
        }

        [Fact]
        public void JSStringEscape_Tabs_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("hello\tworld");
            Assert.Equal("hello\\tworld", result);
        }

        [Fact]
        public void JSStringEscape_Backslashes_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("hello\\world");
            Assert.Equal("hello\\\\world", result);
        }

        [Fact]
        public void JSStringEscape_MixedSpecialCharacters_AreEscaped()
        {
            var result = WebUtil.JSStringEscape("const str = \"hello\n\t'world'\r\";");
            Assert.Equal("const str = \\\"hello\\n\\t\\'world\\'\\r\\\";", result);
        }
    }
}
