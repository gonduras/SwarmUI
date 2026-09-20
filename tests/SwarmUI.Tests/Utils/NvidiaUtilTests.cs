#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using SwarmUI.Core;
using SwarmUI.Utils;
using Xunit;

namespace SwarmUI.Tests.Utils;

[Collection("Sequential")]
public class NvidiaUtilTests : IDisposable
{
    private readonly object _originalQueryLock;
    private readonly bool _originalHasShutdown;
    private readonly Settings _originalSettings;
    private readonly FieldInfo _hasShutdownField;

    // Store original internal state for pristine cleanup
    private readonly bool _originalHasNvidiaGPU;
    private readonly long _originalLastQueryTime;
    private readonly NvidiaUtil.NvidiaInfo[]? _originalLastResultCache;

    public NvidiaUtilTests()
    {
        _originalQueryLock = NvidiaUtil.Internal.QueryLock;
        _originalSettings = Program.ServerSettings;

        _hasShutdownField = typeof(Program).GetField("HasShutdown", BindingFlags.NonPublic | BindingFlags.Static);
        _originalHasShutdown = (bool)_hasShutdownField.GetValue(null)!;

        _originalHasNvidiaGPU = NvidiaUtil.Internal.HasNvidiaGPU;
        _originalLastQueryTime = NvidiaUtil.Internal.LastQueryTime;
        _originalLastResultCache = NvidiaUtil.Internal.LastResultCache;

        Program.ServerSettings = new Settings();

        NvidiaUtil.Internal.HasNvidiaGPU = true;
        NvidiaUtil.Internal.LastResultCache = null;
        NvidiaUtil.Internal.LastQueryTime = 0;
    }

    public void Dispose()
    {
        NvidiaUtil.Internal.QueryLock = (LockObject)_originalQueryLock;
        _hasShutdownField.SetValue(null, _originalHasShutdown);
        Program.ServerSettings = _originalSettings;
        Program.TestShutdownHook = null;

        // Restore pristine state
        NvidiaUtil.Internal.HasNvidiaGPU = _originalHasNvidiaGPU;
        NvidiaUtil.Internal.LastQueryTime = _originalLastQueryTime;
        NvidiaUtil.Internal.LastResultCache = _originalLastResultCache;
    }

    [Fact]
    public void QueryNvidia_WhenExceptionOccurs_SetsHasNvidiaGPUToFalse()
    {
        NvidiaUtil.Internal.QueryLock = null; // Forces ArgumentNullException in lock statement

        var result = NvidiaUtil.QueryNvidia();

        Assert.Null(result);
        Assert.False(NvidiaUtil.Internal.HasNvidiaGPU);
    }

    [Fact]
    public async Task QueryNvidia_WhenExceptionOccursWithPreviousValidResult_RequestsRestartIfEnabled()
    {
        // Arrange
        NvidiaUtil.Internal.QueryLock = null; // Forces exception
        NvidiaUtil.Internal.LastQueryTime = Environment.TickCount64 - 1000;
        Program.ServerSettings.Maintenance.RestartOnGpuCriticalError = true;

        // Ensure HasShutdown starts false
        _hasShutdownField.SetValue(null, false);

        bool shutdownRequested = false;
        var tcs = new TaskCompletionSource();

        Program.TestShutdownHook = (code) =>
        {
            shutdownRequested = true;
            tcs.SetResult();
        };

        // Act
        var result = NvidiaUtil.QueryNvidia();

        // Wait for the background task to hit our hook
        await Task.WhenAny(tcs.Task, Task.Delay(2000));

        // Assert
        Assert.Null(result);
        Assert.False(NvidiaUtil.Internal.HasNvidiaGPU);
        Assert.True(shutdownRequested);
    }

    [Fact]
    public async Task QueryNvidia_WhenExceptionOccursWithPreviousValidResult_DoesNotRequestRestartIfDisabled()
    {
        // Arrange
        NvidiaUtil.Internal.QueryLock = null; // Forces exception
        NvidiaUtil.Internal.LastQueryTime = Environment.TickCount64 - 1000;
        Program.ServerSettings.Maintenance.RestartOnGpuCriticalError = false;

        // Ensure HasShutdown starts false
        _hasShutdownField.SetValue(null, false);

        bool shutdownRequested = false;
        Program.TestShutdownHook = (code) =>
        {
            shutdownRequested = true;
        };

        // Act
        var result = NvidiaUtil.QueryNvidia();

        // Wait a short time to ensure no shutdown is requested
        await Task.Delay(50);

        // Assert
        Assert.Null(result);
        Assert.False(NvidiaUtil.Internal.HasNvidiaGPU);
        Assert.False(shutdownRequested);
    }
}
