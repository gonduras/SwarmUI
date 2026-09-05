using FreneticUtilities.FreneticDataSyntax;
using Newtonsoft.Json.Linq;
using SwarmUI.Accounts;
using SwarmUI.Backends;
using SwarmUI.Core;
using SwarmUI.WebAPI;
using System.IO;

namespace SwarmUI.Tests;

public class BackendAPITests : IDisposable
{
    private readonly string _testDataDir;
    private readonly bool _originalLockSettings;
    private readonly string _oldCurrentDir;
    private readonly SessionHandler _sessionHandler;

    public BackendAPITests()
    {
        // Setup mock environment safely
        _originalLockSettings = Program.LockSettings;
        Program.ServerSettings = new Settings();
        Program.Backends = new BackendHandler();

        _testDataDir = Path.Combine(Path.GetTempPath(), "SwarmUI_Test_Data_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataDir);
        _oldCurrentDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_testDataDir);
        Directory.CreateDirectory("Data"); // Needed for SessionHandler to create Users.ldb
        _sessionHandler = new SessionHandler();
    }

    public void Dispose()
    {
        // Must dispose session handler to release file locks on LiteDB
        // (Wait, there is no Dispose method on SessionHandler, LiteDatabase might need disposing)
        // Let's just restore directory and ignore if temp folder can't be deleted immediately on Windows
        Directory.SetCurrentDirectory(_oldCurrentDir);

        // Clean up mock environment
        Program.LockSettings = _originalLockSettings;
        Program.Backends.T2IBackends.Clear();
        try
        {
            if (Directory.Exists(_testDataDir))
            {
                Directory.Delete(_testDataDir, true);
            }
        }
        catch (IOException)
        {
            // Ignore lock errors from LiteDB if it wasn't disposed cleanly
        }
    }

    [Fact]
    public async Task EditBackend_SettingsLocked_ReturnsError()
    {
        // Arrange
        Program.LockSettings = true;
        var userEntry = new User.DatabaseEntry { ID = "test_user" };
        var user = new User(_sessionHandler, userEntry);
        var session = new Session { User = user };
        var raw_inp = new JObject();

        try
        {
            // Act
            var result = await BackendAPI.EditBackend(session, 1, "New Title", raw_inp);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Settings are locked.", result["error"]?.ToString());
        }
        finally
        {
            Program.LockSettings = false;
        }
    }

    [Fact]
    public async Task EditBackend_MissingSettings_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var userEntry = new User.DatabaseEntry { ID = "test_user" };
        var user = new User(_sessionHandler, userEntry);
        var session = new Session { User = user };
        var raw_inp = new JObject(); // Missing 'settings' property

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "New Title", raw_inp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Missing settings.", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_NewIdAlreadyInUse_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var userEntry = new User.DatabaseEntry { ID = "test_user" };
        var user = new User(_sessionHandler, userEntry);
        var session = new Session { User = user };

        var raw_inp = new JObject();
        raw_inp["settings"] = new JObject();

        // Mock existing backends
        Program.Backends.T2IBackends[1] = new BackendHandler.T2IBackendData(); // Source backend
        Program.Backends.T2IBackends[2] = new BackendHandler.T2IBackendData(); // Target backend

        // Act
        var result = await BackendAPI.EditBackend(session, 1, "New Title", raw_inp, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Backend ID 2 is already in use.", result["error"]?.ToString());
    }

    [Fact]
    public async Task EditBackend_InvalidBackendId_ReturnsError()
    {
        // Arrange
        Program.LockSettings = false;
        var userEntry = new User.DatabaseEntry { ID = "test_user" };
        var user = new User(_sessionHandler, userEntry);
        var session = new Session { User = user };

        var raw_inp = new JObject();
        raw_inp["settings"] = new JObject();

        // Ensure backend 999 doesn't exist
        Program.Backends.T2IBackends.TryRemove(999, out _);

        // Act
        var result = await BackendAPI.EditBackend(session, 999, "New Title", raw_inp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Invalid backend ID 999", result["error"]?.ToString());
    }
}
