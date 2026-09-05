using Xunit;
using SwarmUI.WebAPI;
using SwarmUI.Accounts;
using SwarmUI.Core;
using SwarmUI.Backends;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using FreneticUtilities.FreneticDataSyntax;
using System.Collections.Generic;

namespace SwarmUITests.WebAPI;

public class BackendAPITests
{
    public BackendAPITests()
    {
        // Ensure the directory exists so LiteDB doesn't crash on SessionHandler creation
        Directory.CreateDirectory("Data");
    }

    private Session CreateDummySession()
    {
        var sessionHandler = new SessionHandler();
        sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
        return new Session { User = new User(sessionHandler, new User.DatabaseEntry { ID = "test_user" }) };
    }

    [Fact]
    public async Task EditBackend_SettingsLocked_ReturnsError()
    {
        // Arrange
        Program.LockSettings = true;
        var session = CreateDummySession();
        var rawInp = new JObject();

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "test title", rawInp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Settings are locked.", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_MissingSettings_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var session = CreateDummySession();
        var rawInp = new JObject(); // Missing "settings" property

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "test title", rawInp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Missing settings.", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_NewIdAlreadyInUse_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var session = CreateDummySession();
        var rawInp = new JObject { ["settings"] = new JObject() };

        Program.Backends = new BackendHandler();
        Program.Backends.T2IBackends.TryAdd(2, new BackendHandler.T2IBackendData());

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "test title", rawInp, new_id: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Backend ID 2 is already in use.", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_InvalidBackendId_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var session = CreateDummySession();
        var rawInp = new JObject { ["settings"] = new JObject() };

        Program.Backends = new BackendHandler();
        // Don't add anything to Backends, so ID 1 is invalid

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "test title", rawInp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invalid backend ID 1", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_SameId_ReturnsInvalidBackendIdIfNotFound()
    {
        // Arrange
        Program.LockSettings = false;
        var session = CreateDummySession();
        var rawInp = new JObject { ["settings"] = new JObject() };

        Program.Backends = new BackendHandler();

        // Act
        // new_id == backend_id (1 == 1) should be handled cleanly, converting new_id to -1 internally
        var result = await BackendAPI.EditBackend(session, 1, "test title", rawInp, new_id: 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invalid backend ID 1", result["error"]?.ToString());
    }
}
