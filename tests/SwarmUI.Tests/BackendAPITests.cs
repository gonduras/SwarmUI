using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using SwarmUI.WebAPI;
using SwarmUI.Core;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using System.IO;

namespace SwarmUI.Tests.WebAPITests
{
    public class BackendAPITests
    {
        private Session CreateTestSession()
        {
            // Ensure data directory exists for LiteDB
            Directory.CreateDirectory("Data");

            var dbEntry = new User.DatabaseEntry { ID = "test_user" };
            var sessionHandler = new SessionHandler();
            var user = new User(sessionHandler, dbEntry);
            return new Session { User = user };
        }

        [Fact]
        public async Task RestartBackends_SettingsLocked_ReturnsError()
        {
            // Arrange
            Program.LockSettings = true;
            var session = CreateTestSession();

            // Act
            var result = await BackendAPI.RestartBackends(session, "all");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("error"));
            Assert.Equal("Settings are locked.", result["error"]?.ToString());

            // Cleanup
            Program.LockSettings = false;
        }

        [Fact]
        public async Task RestartBackends_SpecificBackendNotFound_ReturnsZeroCount()
        {
            // Arrange
            Program.LockSettings = false;
            Program.Backends = new BackendHandler();
            var session = CreateTestSession();

            // Act
            var result = await BackendAPI.RestartBackends(session, "nonexistent_backend");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("result"));
            Assert.Equal("Success.", result["result"]?.ToString());
            Assert.True(result.ContainsKey("count"));
            if (result["count"] != null)
            {
                Assert.Equal(0, (int)result["count"]!);
            }
        }
    }
}
