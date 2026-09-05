using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using FreneticUtilities.FreneticDataSyntax;

namespace SwarmUI.Tests
{
    public class DummyBackend : AbstractT2IBackend
    {
        public override Task Init() => Task.CompletedTask;
        public override Task Shutdown() => Task.CompletedTask;
        public override Task<SwarmUI.Utils.Image[]> Generate(T2IParamInput user_input) => Task.FromResult(Array.Empty<SwarmUI.Utils.Image>());
        public override IEnumerable<string> SupportedFeatures => Array.Empty<string>();

        public class DummySettings : AutoConfiguration { }
    }

    public class BackendAPITests
    {
        private BackendHandler.BackendType dummyBackendType;

        public BackendAPITests()
        {
            Directory.CreateDirectory("Data");
            Program.ServerSettings = new Settings();
            Program.Backends = new BackendHandler();

            // To prevent NullReferenceException in BackendData.BackType.Name
            dummyBackendType = new BackendHandler.BackendType(
                "dummy_id",
                "DummyBackend",
                "Desc",
                typeof(DummyBackend.DummySettings),
                null,
                typeof(DummyBackend),
                new JObject(),
                false
            );
        }

        [Fact]
        public async Task RestartBackends_ReturnsError_WhenSettingsAreLocked()
        {
            // Arrange
            var sessionHandler = new SessionHandler();
            var userData = new User.DatabaseEntry { ID = "testuser" };
            var user = new User(sessionHandler, userData);
            var session = new Session { User = user };
            Program.LockSettings = true;

            // Act
            var result = await BackendAPI.RestartBackends(session, "all");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Settings are locked.", result["error"]?.ToString());
        }

        [Fact]
        public async Task RestartBackends_RestartsAllBackends_WhenBackendArgIsAll()
        {
            // Arrange
            var sessionHandler = new SessionHandler();
            var userData = new User.DatabaseEntry { ID = "testuser" };
            var user = new User(sessionHandler, userData);
            var session = new Session { User = user };
            Program.LockSettings = false;

            var dummyBackend1 = new DummyBackend { Status = BackendStatus.RUNNING, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData1 = new BackendHandler.T2IBackendData
            {
                ID = 1,
                Backend = dummyBackend1,
                BackType = dummyBackendType
            };
            var dummyBackend2 = new DummyBackend { Status = BackendStatus.ERRORED, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData2 = new BackendHandler.T2IBackendData
            {
                ID = 2,
                Backend = dummyBackend2,
                BackType = dummyBackendType
            };

            Program.Backends.T2IBackends.Clear();
            Program.Backends.T2IBackends.TryAdd(1, backendData1);
            Program.Backends.T2IBackends.TryAdd(2, backendData2);

            // Act
            var result = await BackendAPI.RestartBackends(session, "all");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success.", result["result"]?.ToString());
            Assert.Equal(2, result["count"]?.ToObject<int>());
        }

        [Fact]
        public async Task RestartBackends_RestartsSpecificBackend_WhenBackendArgIsId()
        {
            // Arrange
            var sessionHandler = new SessionHandler();
            var userData = new User.DatabaseEntry { ID = "testuser" };
            var user = new User(sessionHandler, userData);
            var session = new Session { User = user };
            Program.LockSettings = false;

            var dummyBackend1 = new DummyBackend { Status = BackendStatus.RUNNING, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData1 = new BackendHandler.T2IBackendData
            {
                ID = 1,
                Backend = dummyBackend1,
                BackType = dummyBackendType
            };
            var dummyBackend2 = new DummyBackend { Status = BackendStatus.RUNNING, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData2 = new BackendHandler.T2IBackendData
            {
                ID = 2,
                Backend = dummyBackend2,
                BackType = dummyBackendType
            };

            Program.Backends.T2IBackends.Clear();
            Program.Backends.T2IBackends.TryAdd(1, backendData1);
            Program.Backends.T2IBackends.TryAdd(2, backendData2);

            // Act
            var result = await BackendAPI.RestartBackends(session, "1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success.", result["result"]?.ToString());
            Assert.Equal(1, result["count"]?.ToObject<int>());
        }

        [Fact]
        public async Task RestartBackends_IgnoresDisabledBackends()
        {
            // Arrange
            var sessionHandler = new SessionHandler();
            var userData = new User.DatabaseEntry { ID = "testuser" };
            var user = new User(sessionHandler, userData);
            var session = new Session { User = user };
            Program.LockSettings = false;

            var dummyBackend1 = new DummyBackend { Status = BackendStatus.DISABLED, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData1 = new BackendHandler.T2IBackendData
            {
                ID = 1,
                Backend = dummyBackend1,
                BackType = dummyBackendType
            };
            var dummyBackend2 = new DummyBackend { Status = BackendStatus.LOADING, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData2 = new BackendHandler.T2IBackendData
            {
                ID = 2,
                Backend = dummyBackend2,
                BackType = dummyBackendType
            };
            var dummyBackend3 = new DummyBackend { Status = BackendStatus.IDLE, BackendData = new BackendHandler.T2IBackendData { BackType = dummyBackendType } };
            var backendData3 = new BackendHandler.T2IBackendData
            {
                ID = 3,
                Backend = dummyBackend3,
                BackType = dummyBackendType
            };

            Program.Backends.T2IBackends.Clear();
            Program.Backends.T2IBackends.TryAdd(1, backendData1);
            Program.Backends.T2IBackends.TryAdd(2, backendData2);
            Program.Backends.T2IBackends.TryAdd(3, backendData3);

            // Act
            var result = await BackendAPI.RestartBackends(session, "all");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success.", result["result"]?.ToString());
            Assert.Equal(0, result["count"]?.ToObject<int>());
        }
    }
}
