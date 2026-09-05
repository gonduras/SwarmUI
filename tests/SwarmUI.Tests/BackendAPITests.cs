using System.Threading.Tasks;
using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Backends;
using SwarmUI.Accounts;
using SwarmUI.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using FreneticUtilities.FreneticDataSyntax;

namespace SwarmUI.Tests;

public class BackendAPITests
{
    // Mock backend to inject for testing
    public class MockBackend : AbstractT2IBackend
    {
        public bool DidFreeMemory = false;
        public bool GotSystemRam = false;

        public override Task Init() => Task.CompletedTask;
        public override Task Shutdown() => Task.CompletedTask;
        public override Task<SwarmUI.Utils.Image[]> Generate(SwarmUI.Text2Image.T2IParamInput user_input) => Task.FromResult(new SwarmUI.Utils.Image[0]);
        public override System.Collections.Generic.IEnumerable<string> SupportedFeatures => [];

        public override Task<bool> FreeMemory(bool systemRam)
        {
            DidFreeMemory = true;
            GotSystemRam = systemRam;
            return Task.FromResult(true);
        }
    }

    [Fact]
    public async Task TestFreeBackendMemory_NoSystemRam_AllBackends()
    {
        // Setup minimal environment
        Program.Backends = new BackendHandler();

        var mockBackend = new MockBackend() { BackendData = new BackendHandler.T2IBackendData() { ID = 1 }, Status = BackendStatus.RUNNING };
        Program.Backends.T2IBackends.TryAdd(1, mockBackend.BackendData);
        mockBackend.BackendData.Backend = mockBackend;

        Session.RecentlyBlockedFilenames = new ConcurrentDictionary<string, string>();
        Session.RecentlyBlockedFilenames.TryAdd("test", "test");

        var result = await BackendAPI.FreeBackendMemory(null, system_ram: false, backend: "all");

        Assert.True(result["result"]?.Value<bool>());
        Assert.Equal(1, result["count"]?.Value<int>());
        Assert.True(mockBackend.DidFreeMemory);
        Assert.False(mockBackend.GotSystemRam);

        // Ensure system RAM wasn't cleared
        Assert.Single(Session.RecentlyBlockedFilenames);
    }

    [Fact]
    public async Task TestFreeBackendMemory_WithSystemRam_SpecificBackend()
    {
        // Setup minimal environment
        Program.Backends = new BackendHandler();

        var mockBackend1 = new MockBackend() { BackendData = new BackendHandler.T2IBackendData() { ID = 1 }, Status = BackendStatus.RUNNING };
        var mockBackend2 = new MockBackend() { BackendData = new BackendHandler.T2IBackendData() { ID = 2 }, Status = BackendStatus.RUNNING };
        Program.Backends.T2IBackends.TryAdd(1, mockBackend1.BackendData);
        Program.Backends.T2IBackends.TryAdd(2, mockBackend2.BackendData);
        mockBackend1.BackendData.Backend = mockBackend1;
        mockBackend2.BackendData.Backend = mockBackend2;

        Session.RecentlyBlockedFilenames = new ConcurrentDictionary<string, string>();
        Session.RecentlyBlockedFilenames.TryAdd("test", "test");

        var result = await BackendAPI.FreeBackendMemory(null, system_ram: true, backend: "2");

        Assert.True(result["result"]?.Value<bool>());
        Assert.Equal(1, result["count"]?.Value<int>());
        Assert.False(mockBackend1.DidFreeMemory);
        Assert.True(mockBackend2.DidFreeMemory);
        Assert.True(mockBackend2.GotSystemRam);

        // Ensure system RAM was cleared
        Assert.Empty(Session.RecentlyBlockedFilenames);
    }

    [Fact]
    public async Task TestFreeBackendMemory_NoMatchingBackend()
    {
        // Setup minimal environment
        Program.Backends = new BackendHandler();

        var mockBackend = new MockBackend() { BackendData = new BackendHandler.T2IBackendData() { ID = 1 }, Status = BackendStatus.RUNNING };
        Program.Backends.T2IBackends.TryAdd(1, mockBackend.BackendData);
        mockBackend.BackendData.Backend = mockBackend;

        var result = await BackendAPI.FreeBackendMemory(null, system_ram: false, backend: "999");

        Assert.False(result["result"]?.Value<bool>());
        Assert.Equal(0, result["count"]?.Value<int>());
        Assert.False(mockBackend.DidFreeMemory);
    }
}
