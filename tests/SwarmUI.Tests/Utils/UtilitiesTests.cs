using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwarmUI.Utils;
using Xunit;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Fact]
        public void ParseToJson_ValidJson_ReturnsJObject()
        {
            // Arrange
            string input = "{\"key\": \"value\"}";

            // Act
            JObject result = input.ParseToJson();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("value", result["key"]?.ToString());
        }

        [Fact]
        public void ParseToJson_MalformedJson_ThrowsJsonReaderExceptionWithCustomMessage()
        {
            // Arrange
            string input = "{\"key\": \"value\""; // Missing closing brace

            // Act & Assert
            var exception = Assert.Throws<JsonReaderException>(() => input.ParseToJson());
            Assert.Contains("Failed to parse JSON", exception.Message);
            Assert.Contains(input, exception.Message);
        }

        [Fact]
        public void ParseToJson_MalformedJsonWithNewlines_ThrowsJsonReaderExceptionWithReplacedNewlines()
        {
            // Arrange
            string input = "{\n\"key\": \n\"value\""; // Missing closing brace and has newlines

            // Act & Assert
            var exception = Assert.Throws<JsonReaderException>(() => input.ParseToJson());
            Assert.Contains("Failed to parse JSON", exception.Message);
            Assert.Contains("{  \"key\":   \"value\"", exception.Message);
            Assert.DoesNotContain("\n", exception.Message);
        }
    }
}
