using System;
using System.IO;
using Xunit;
using SwarmUI.Core;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Core;

public class WebServerTests
{
    [Theory]
    [InlineData("/var/www/root", "test.png", "/var/www/root/test.png", true)]
    [InlineData("/var/www/root", "sub/test.png", "/var/www/root/sub/test.png", true)]
    [InlineData("/var/www/root", "../root/test.png", "/var/www/root/test.png", true)]
    [InlineData("/var/www/root", "../../root2/test.png", null, false)]
    [InlineData("/var/www/root", "/../rootetc/test.png", null, false)] // Exploitative path
    [InlineData("/var/www/root", "./././test.png", "/var/www/root/test.png", true)]
    public void CheckFilePath_SecurityCheck(string root, string path, string expectedPath, bool shouldSucceed)
    {
        // Normalize paths for cross-platform
        root = root.Replace('\\', '/');
        if (expectedPath != null) expectedPath = expectedPath.Replace('\\', '/');

        var (resultPath, consoleError, userError) = WebServer.CheckFilePath(root, path);

        if (shouldSucceed)
        {
            Assert.NotNull(resultPath);
            Assert.Null(consoleError);
            Assert.Null(userError);
            Assert.Equal(expectedPath, resultPath.Replace('\\', '/'));
        }
        else
        {
            Assert.Null(resultPath);
            Assert.NotNull(consoleError);
            Assert.NotNull(userError);
        }
    }
}
