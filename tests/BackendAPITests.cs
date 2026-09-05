using System;
using System.Threading.Tasks;
using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Accounts;
using SwarmUI.Core;
using SwarmUI.Backends;
using SwarmUI.Utils;
using SwarmUI.Text2Image;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SwarmUI.Tests
{
    public class DummyBackend : AbstractT2IBackend
    {
        public bool MemoryFreed = false;
        public bool SystemRamFreed = false;

        public override Task Init() { return Task.CompletedTask; }
        public override Task Shutdown() { return Task.CompletedTask; }
        public override Task<bool> LoadModel(T2IModel model, T2IParamInput input) { return Task.FromResult(true); }
        public override Task<bool> FreeMemory(bool systemRam)
        {
            MemoryFreed = true;
            SystemRamFreed = systemRam;
            return Task.FromResult(true);
        }

        public override IEnumerable<string> SupportedFeatures => Array.Empty<string>();

        public override Task<Image[]> Generate(T2IParamInput user_input) { return Task.FromResult(Array.Empty<Image>()); }
    }

    public class BackendAPITests
    {
        public BackendAPITests()
        {
            if (Program.Backends == null)
            {
                Program.Backends = new BackendHandler();
            }
            if (Program.ServerSettings == null)
            {
                Program.ServerSettings = new Settings();
            }
        }

        private Session CreateTestSession()
        {
            return new Session() { ID = "test" };
        }

        [Fact]
        public async Task TestFreeBackendMemory_All()
        {
            var dummy1 = new DummyBackend();
            dummy1.Status = BackendStatus.RUNNING;
            dummy1.BackendData = new BackendHandler.T2IBackendData() { ID = 1, Backend = dummy1 };

            var dummy2 = new DummyBackend();
            dummy2.Status = BackendStatus.RUNNING;
            dummy2.BackendData = new BackendHandler.T2IBackendData() { ID = 2, Backend = dummy2 };

            Program.Backends.T2IBackends[1] = dummy1.BackendData;
            Program.Backends.T2IBackends[2] = dummy2.BackendData;

            var session = CreateTestSession();

            var result = await BackendAPI.FreeBackendMemory(session, false, "all");

            Assert.True((bool)result["result"]);
            Assert.Equal(2, (int)result["count"]);
            Assert.True(dummy1.MemoryFreed);
            Assert.True(dummy2.MemoryFreed);
            Assert.False(dummy1.SystemRamFreed);
            Assert.False(dummy2.SystemRamFreed);

            Program.Backends.T2IBackends.Clear();
        }

        [Fact]
        public async Task TestFreeBackendMemory_Specific()
        {
            var dummy1 = new DummyBackend();
            dummy1.Status = BackendStatus.RUNNING;
            dummy1.BackendData = new BackendHandler.T2IBackendData() { ID = 1, Backend = dummy1 };

            var dummy2 = new DummyBackend();
            dummy2.Status = BackendStatus.RUNNING;
            dummy2.BackendData = new BackendHandler.T2IBackendData() { ID = 2, Backend = dummy2 };

            Program.Backends.T2IBackends[1] = dummy1.BackendData;
            Program.Backends.T2IBackends[2] = dummy2.BackendData;

            var session = CreateTestSession();

            var result = await BackendAPI.FreeBackendMemory(session, true, "1");

            Assert.True((bool)result["result"]);
            Assert.Equal(1, (int)result["count"]);
            Assert.True(dummy1.MemoryFreed);
            Assert.False(dummy2.MemoryFreed);
            Assert.True(dummy1.SystemRamFreed);
            Assert.False(dummy2.SystemRamFreed);

            Program.Backends.T2IBackends.Clear();
        }

        [Fact]
        public async Task TestFreeBackendMemory_None()
        {
            Program.Backends.T2IBackends.Clear();

            var session = CreateTestSession();

            var result = await BackendAPI.FreeBackendMemory(session, false, "all");

            Assert.False((bool)result["result"]);
            Assert.Equal(0, (int)result["count"]);
        }
    }
}
