using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Core;
using SwarmUI.Utils;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using SwarmUI.DataHolders;
using SwarmUI.Text2Image;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

namespace SwarmUI.Tests.WebAPI
{
    public class BackendAPITests
    {
        public BackendAPITests()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
        }

        private class MockBackend : AbstractT2IBackend
        {
            public override Task Init() => Task.CompletedTask;
            public override Task Shutdown() => Task.CompletedTask;
            public override Task<Image[]> Generate(T2IParamInput user_input) => Task.FromResult(new Image[0]);
            public override IEnumerable<string> SupportedFeatures => new string[0];
        }

        private Session CreateMockSession()
        {
            var sessionHandler = new SessionHandler();
            sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
            var user = new User(sessionHandler, new User.DatabaseEntry { ID = "test_user" });
            return new Session() { User = user };
        }

        [Fact]
        public async Task ToggleBackend_WhenSettingsAreLocked_ReturnsError()
        {
            // Arrange
            Program.LockSettings = true;
            var session = CreateMockSession();

            // Act
            var result = await BackendAPI.ToggleBackend(session, 1, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Settings are locked.", result["error"]?.ToString());

            // Cleanup
            Program.LockSettings = false;
        }

        [Fact]
        public async Task ToggleBackend_WhenBackendIdIsInvalid_ReturnsError()
        {
            // Arrange
            Program.LockSettings = false;
            Program.Backends = new BackendHandler();
            Program.Backends.T2IBackends = new ConcurrentDictionary<int, BackendHandler.T2IBackendData>();
            var session = CreateMockSession();
            int invalidBackendId = 999;

            // Act
            var result = await BackendAPI.ToggleBackend(session, invalidBackendId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"Invalid backend ID {invalidBackendId}", result["error"]?.ToString());
        }

        [Fact]
        public async Task ToggleBackend_WhenNoChange_ReturnsNoChange()
        {
            // Arrange
            Program.LockSettings = false;
            Program.Backends = new BackendHandler();
            Program.Backends.T2IBackends = new ConcurrentDictionary<int, BackendHandler.T2IBackendData>();

            var mockBackend = new MockBackend() { IsEnabled = true };
            var backendData = new BackendHandler.T2IBackendData()
            {
                Backend = mockBackend
            };

            Program.Backends.T2IBackends[1] = backendData;

            var session = CreateMockSession();

            // Act
            var result = await BackendAPI.ToggleBackend(session, 1, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("No change.", result["result"]?.ToString());
        }

        [Fact]
        public async Task ToggleBackend_WhenChanged_ReturnsSuccess()
        {
            // Arrange
            Program.LockSettings = false;
            Program.Backends = new BackendHandler();
            Program.Backends.T2IBackends = new ConcurrentDictionary<int, BackendHandler.T2IBackendData>();

            var mockBackend = new MockBackend() { IsEnabled = false };
            var backendData = new BackendHandler.T2IBackendData()
            {
                Backend = mockBackend
            };

            Program.Backends.T2IBackends[1] = backendData;
            Program.Backends.BackendsToInit = new ConcurrentQueue<BackendHandler.T2IBackendData>();

            var session = CreateMockSession();

            // Act
            var result = await BackendAPI.ToggleBackend(session, 1, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success.", result["result"]?.ToString());
            Assert.True(mockBackend.IsEnabled);
            Assert.Equal(BackendStatus.WAITING, mockBackend.Status);
        }
    }
}
