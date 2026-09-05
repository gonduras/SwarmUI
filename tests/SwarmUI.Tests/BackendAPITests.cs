using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Backends;
using SwarmUI.Core;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using SwarmUI.Utils;
using FreneticUtilities.FreneticDataSyntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace SwarmUI.Tests
{
    public class BackendAPITests
    {
        public BackendAPITests()
        {
            if (Program.Backends == null)
            {
                Program.Backends = new BackendHandler();
            }
        }

        [Fact]
        public async Task ListBackends_EmptyList_ReturnsEmptyJObject()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            // Act
            JObject result = await BackendAPI.ListBackends(null, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Properties());
        }

        private class MockSettings : AutoConfiguration { }

        private class MockAbstractT2IBackend : AbstractT2IBackend
        {
            public override Task Init() { return Task.CompletedTask; }
            public override Task Shutdown() { return Task.CompletedTask; }
            public override Task<SwarmUI.Utils.Image[]> Generate(SwarmUI.Text2Image.T2IParamInput user_input) { return Task.FromResult(new SwarmUI.Utils.Image[0]); }
            public override IEnumerable<string> SupportedFeatures { get { return new string[0]; } }
        }

        private BackendHandler.BackendType CreateMockBackendType(string id)
        {
            return new BackendHandler.BackendType(
                id,
                "Test Backend",
                "A test backend",
                typeof(MockSettings),
                new MockSettings().InternalData.SharedData,
                typeof(MockAbstractT2IBackend),
                new JObject(),
                false
            );
        }

        [Fact]
        public async Task ListBackends_WithRealBackend_ReturnsBackend()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            var backendType = CreateMockBackendType("test_backend");

            var abstractBackend = new MockAbstractT2IBackend();
            abstractBackend.Status = BackendStatus.RUNNING;
            abstractBackend.CurrentModelName = "test_model";
            abstractBackend.SettingsRaw = new MockSettings();

            var backendData = new BackendHandler.T2IBackendData
            {
                ID = 1,
                BackType = backendType,
                Backend = abstractBackend,
                ModCount = 1
            };

            abstractBackend.BackendData = backendData;

            Program.Backends.T2IBackends.TryAdd(1, backendData);

            // Act
            JObject result = await BackendAPI.ListBackends(null, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Properties());
            Assert.True(result.ContainsKey("1"));
            var backendJson = result["1"] as JObject;
            Assert.NotNull(backendJson);
            Assert.Equal("test_backend", backendJson["type"]?.ToString());
            Assert.Equal(1, backendJson["id"]?.Value<int>());
        }

        [Fact]
        public async Task ListBackends_WithNonRealBackend_DoesNotReturnIfNonRealIsFalse()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            var backendType = CreateMockBackendType("test_backend_nonreal");

            var abstractBackend = new MockAbstractT2IBackend();
            abstractBackend.SettingsRaw = new MockSettings();
            abstractBackend.IsReal = false; // Add this line because non-realness is based on IsReal=false

            var backendData = new BackendHandler.T2IBackendData
            {
                ID = -1, // Convention for non-real backends
                BackType = backendType,
                Backend = abstractBackend
            };
            abstractBackend.BackendData = backendData;

            Program.Backends.T2IBackends.TryAdd(-1, backendData);

            // Act
            JObject result = await BackendAPI.ListBackends(null, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Properties());
        }

        [Fact]
        public async Task ListBackends_WithNonRealBackend_ReturnsIfNonRealIsTrue()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            var backendType = CreateMockBackendType("test_backend_nonreal");

            var abstractBackend = new MockAbstractT2IBackend();
            abstractBackend.SettingsRaw = new MockSettings();
            abstractBackend.IsReal = false;

            var backendData = new BackendHandler.T2IBackendData
            {
                ID = -1, // Convention for non-real backends
                BackType = backendType,
                Backend = abstractBackend
            };
            abstractBackend.BackendData = backendData;

            Program.Backends.T2IBackends.TryAdd(-1, backendData);

            // Act
            JObject result = await BackendAPI.ListBackends(null, true, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Properties());
            Assert.True(result.ContainsKey("-1"));
        }

        [Fact]
        public async Task ListBackends_WithFullDataTrue_ReturnsCurrentModel()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            var backendType = CreateMockBackendType("test_backend");

            var abstractBackend = new MockAbstractT2IBackend();
            abstractBackend.Status = BackendStatus.RUNNING;
            abstractBackend.CurrentModelName = "test_model_loaded_name";
            abstractBackend.SettingsRaw = new MockSettings();

            var backendData = new BackendHandler.T2IBackendData
            {
                ID = 2,
                BackType = backendType,
                Backend = abstractBackend,
                ModCount = 1
            };

            abstractBackend.BackendData = backendData;

            Program.Backends.T2IBackends.TryAdd(2, backendData);

            // Act
            JObject result = await BackendAPI.ListBackends(null, false, true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Properties());
            Assert.True(result.ContainsKey("2"));
            var backendJson = result["2"] as JObject;
            Assert.NotNull(backendJson);
            Assert.Equal("test_model_loaded_name", backendJson["current_model"]?.ToString());
        }

        [Fact]
        public async Task ListBackends_WithFullDataFalse_DoesNotReturnCurrentModel()
        {
            // Arrange
            Program.Backends.T2IBackends.Clear();

            var backendType = CreateMockBackendType("test_backend");

            var abstractBackend = new MockAbstractT2IBackend();
            abstractBackend.Status = BackendStatus.RUNNING;
            abstractBackend.CurrentModelName = "test_model_loaded_name";
            abstractBackend.SettingsRaw = new MockSettings();

            var backendData = new BackendHandler.T2IBackendData
            {
                ID = 3,
                BackType = backendType,
                Backend = abstractBackend,
                ModCount = 1
            };

            abstractBackend.BackendData = backendData;

            Program.Backends.T2IBackends.TryAdd(3, backendData);

            // Act
            JObject result = await BackendAPI.ListBackends(null, false, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Properties());
            Assert.True(result.ContainsKey("3"));
            var backendJson = result["3"] as JObject;
            Assert.NotNull(backendJson);
            Assert.Null(backendJson["current_model"]);
        }
    }
}
