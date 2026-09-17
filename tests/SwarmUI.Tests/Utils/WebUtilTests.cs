using Xunit;
using SwarmUI.Utils;
using System.Runtime.InteropServices;

namespace SwarmUI.Tests.Utils
{
    public class WebUtilTests
    {
        [Fact]
        public void IsWindows_ReturnsCorrectValue()
        {
            // Arrange
            bool expected = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // Act
            bool result = WebUtil.IsWindows();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
