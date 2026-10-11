using Xunit;
using SwarmUI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SwarmUI.Tests.Utils
{
    public class UtilitiesTests
    {
        [Fact]
        public void ParseToJson_ValidJson_ReturnsJObject()
        {
            // Arrange
            string validJson = "{\"key\": \"value\"}";

            // Act
            JObject result = validJson.ParseToJson();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("value", result["key"]?.ToString());
        }

        [Fact]
        public void ParseToJson_MalformedJson_ThrowsJsonReaderException()
        {
            // Arrange
            string malformedJson = "{key: \"value\"";

            // Act & Assert
            var ex = Assert.Throws<JsonReaderException>(() => malformedJson.ParseToJson());
            Assert.Contains("Failed to parse JSON", ex.Message);
        }
    }
}
