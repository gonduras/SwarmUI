using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Xunit;
using SwarmUI.Core;
using SwarmUI.Accounts;
using SwarmUI.Utils;
using FreneticUtilities.FreneticExtensions;
using SwarmUI.WebAPI;
using SwarmUI.Backends;
using SwarmUI.Text2Image;

namespace SwarmUI.Tests.WebAPI
{
    [Collection("Sequential")]
    public class BackendAPITests : IDisposable
    {
        private readonly string _tempDataDir;
        private readonly SessionHandler _sessionHandler;

        public BackendAPITests()
        {
            _tempDataDir = Path.Combine(Path.GetTempPath(), "SwarmUI_Test_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataDir);

            Program.DataDir = _tempDataDir;
            Program.ServerSettings = new Settings();
            Program.NoPersist = false;

            _sessionHandler = new SessionHandler();
            _sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
        }

        public void Dispose()
        {
            _sessionHandler.Shutdown();
            if (Directory.Exists(_tempDataDir))
            {
                Directory.Delete(_tempDataDir, true);
            }
        }

        private class MockBackend : AbstractT2IBackend
        {
            public override Task Init() => Task.CompletedTask;
            public override Task Shutdown() => Task.CompletedTask;
            public override Task<SwarmUI.Utils.Image[]> Generate(T2IParamInput user_input) => Task.FromResult(new SwarmUI.Utils.Image[0]);
            public override IEnumerable<string> SupportedFeatures => new string[0];
        }

        private Session CreateMockSession()
        {
            var user = new User(_sessionHandler, new User.DatabaseEntry { ID = "test_user" });
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
