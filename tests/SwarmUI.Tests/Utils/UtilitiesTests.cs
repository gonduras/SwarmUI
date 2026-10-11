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
            string input = "{\"key\": \"value\"}";

            // Act
            JObject result = input.ParseToJson();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("value", result["key"]?.ToString());
        }

        [Fact]
        public void ParseToJson_InvalidJson_ThrowsJsonReaderExceptionWithFormattedMessage()
        {
            // Arrange
            string input = "invalid\njson";

            // Act & Assert
            var exception = Assert.Throws<JsonReaderException>(() => input.ParseToJson());

            // The method replaces "\n" with "  " before cleaning
            // "invalid  json"
            Assert.Contains("Failed to parse JSON `invalid  json`:", exception.Message);
        }
    }
}
