using System;
using System.IO;
using System.Threading;
using SwarmUI.Utils;
using SwarmUI.Core;
using Xunit;

namespace SwarmUI.Tests.Utils
{
    [Collection("Sequential")]
    public class LogsTests : IDisposable
    {
        private readonly string _originalDataDir;
        private readonly string _testDataDir;
        private readonly bool _originalSaveLogToFile;
        private readonly string _originalLogsPath;
        private readonly CancellationTokenSource _testCancelSource;
        private readonly Thread _originalLogSaveThread;
        private readonly ManualResetEvent _originalLogSaveCompletion;
        private readonly CancellationToken _originalGlobalCancel;
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> _originalLogsToSave;

        public LogsTests()
        {
            // Setup isolated state
            _originalDataDir = Program.DataDir;
            _testDataDir = Path.Combine(Path.GetTempPath(), "SwarmUI_LogsTest_" + Guid.NewGuid().ToString());

            // Ensure the directory exists before assigning it, to prevent side-effects with SessionHandler and other singletons
            Directory.CreateDirectory(_testDataDir);
            Program.DataDir = _testDataDir;

            if (Program.ServerSettings == null)
            {
                Program.ServerSettings = new Settings();
            }

            _originalSaveLogToFile = Program.ServerSettings.Logs.SaveLogToFile;
            _originalLogsPath = Program.ServerSettings.Logs.LogsPath;

            _originalLogSaveThread = Logs.LogSaveThread;
            _originalLogSaveCompletion = Logs.LogSaveCompletion;
            _originalGlobalCancel = Program.GlobalProgramCancel;
            _originalLogsToSave = Logs.LogsToSave;

            // We need to override GlobalProgramCancel to control the loop in testing without shutting down the actual app if it was running.
            // Since GlobalProgramCancel is a static field, it's risky to replace it if other tests use it concurrently, hence [Collection("Sequential")]
            _testCancelSource = new CancellationTokenSource();
            Program.GlobalProgramCancel = _testCancelSource.Token;

            // Ensure thread is not running
            if (Logs.LogSaveThread != null && Logs.LogSaveThread.IsAlive)
            {
                _testCancelSource.Cancel();
                Logs.LogSaveCompletion.WaitOne(5000);
            }
            Logs.LogSaveThread = null;

            // Reset state
            _testCancelSource = new CancellationTokenSource();
            Program.GlobalProgramCancel = _testCancelSource.Token;
            Logs.LogSaveCompletion = new ManualResetEvent(false);
            Logs.LogsToSave = new System.Collections.Concurrent.ConcurrentQueue<string>();
        }

        public void Dispose()
        {
            // Teardown
            _testCancelSource.Cancel();
            if (Logs.LogSaveThread != null && Logs.LogSaveThread.IsAlive)
            {
                Logs.LogSaveCompletion.WaitOne(5000);
            }

            Program.ServerSettings.Logs.SaveLogToFile = _originalSaveLogToFile;
            Program.ServerSettings.Logs.LogsPath = _originalLogsPath;
            Program.DataDir = _originalDataDir;
            Logs.LogSaveThread = _originalLogSaveThread;
            Logs.LogSaveCompletion = _originalLogSaveCompletion;
            Program.GlobalProgramCancel = _originalGlobalCancel;
            Logs.LogsToSave = _originalLogsToSave;

            if (Directory.Exists(_testDataDir))
            {
                try
                {
                    Directory.Delete(_testDataDir, true);
                }
                catch { } // Ignore cleanup errors
            }
        }

        [Fact]
        public void StartLogSaving_GeneratesCorrectFilePath()
        {
            // Arrange
            Program.ServerSettings.Logs.SaveLogToFile = true;
            Program.ServerSettings.Logs.LogsPath = "Logs/test-[year]-[month]-[day]-[hour]-[minute]-[second]-[pid].log";

            DateTimeOffset time = DateTimeOffset.Now;
            string expectedPathFragment = $"Logs/test-{time.Year:0000}-{time.Month:00}-{time.Day:00}-{time.Hour:00}-{time.Minute:00}"; // Omit second/pid to avoid flakiness

            // Act
            Logs.StartLogSaving();

            // Assert
            Assert.NotNull(Logs.LogFilePath);
            Assert.Contains(expectedPathFragment, Logs.LogFilePath);
            Assert.StartsWith(Program.DataDir, Logs.LogFilePath); // Ensures it combined with DataDir
            Assert.True(Directory.Exists(Path.GetDirectoryName(Logs.LogFilePath)));
            Assert.NotNull(Logs.LogSaveThread);
            Assert.True(Logs.LogSaveThread.IsAlive);
        }

        [Fact]
        public void StartLogSaving_WithDisabledSettings_DoesNotStartThread()
        {
            // Arrange
            Program.ServerSettings.Logs.SaveLogToFile = false;

            // Act
            Logs.StartLogSaving();

            // Assert
            Assert.True(Logs.LogSaveCompletion.WaitOne(0)); // Should be set immediately
            Assert.Null(Logs.LogsToSave);
        }
    }
}
