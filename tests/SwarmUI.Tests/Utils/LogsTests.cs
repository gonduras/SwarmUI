using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using SwarmUI.Core;
using SwarmUI.Utils;
using Xunit;

namespace SwarmUI.Tests.Utils
{
    [Collection("Sequential")]
    public class LogsTests : IDisposable
    {
        private readonly string OriginalDataDir;
        private readonly Settings OriginalSettings;

        public LogsTests()
        {
            OriginalDataDir = Program.DataDir;
            OriginalSettings = Program.ServerSettings;

            Program.DataDir = Path.Combine(Path.GetTempPath(), "SwarmUI_LogsTests_" + Guid.NewGuid().ToString());

            // Ensure the temp directory exists
            Directory.CreateDirectory(Program.DataDir);

            Program.ServerSettings = new Settings();
        }

        public void Dispose()
        {
            try
            {
                // Force exit log saver to prevent thread from crashing during tests
                var globalCancelSourceField = typeof(Program).GetField("GlobalCancelSource", BindingFlags.Static | BindingFlags.NonPublic);
                var originalGlobalCancelSource = globalCancelSourceField?.GetValue(null) as CancellationTokenSource;

                // Create a temporary cancel source to kill the thread without affecting the original
                using (var tempCancelSource = new CancellationTokenSource())
                {
                    if (globalCancelSourceField != null)
                    {
                        globalCancelSourceField.SetValue(null, tempCancelSource);
                        Program.GlobalProgramCancel = tempCancelSource.Token;
                    }

                    tempCancelSource.Cancel();

                    if (Logs.LogSaveThread != null && Logs.LogSaveThread.IsAlive)
                    {
                        Logs.LogSaveCompletion.WaitOne(TimeSpan.FromSeconds(2));
                    }
                }

                if (Directory.Exists(Program.DataDir))
                {
                    Directory.Delete(Program.DataDir, true);
                }

                // Restore the original token source
                if (globalCancelSourceField != null && originalGlobalCancelSource != null)
                {
                    globalCancelSourceField.SetValue(null, originalGlobalCancelSource);
                    Program.GlobalProgramCancel = originalGlobalCancelSource.Token;
                }
            }
            catch { }

            Program.DataDir = OriginalDataDir;
            Program.ServerSettings = OriginalSettings;
        }

        [Fact]
        public void StartLogSaving_GeneratesCorrectLogFilePath()
        {
            // Arrange
            Program.ServerSettings.Logs.SaveLogToFile = true;
            Program.ServerSettings.Logs.LogsPath = "Logs/[year]-[month]/[day]-[hour]-[minute].log";

            if (Logs.LogsToSave == null) {
                Logs.LogsToSave = new ConcurrentQueue<string>();
            }

            // Act
            Logs.StartLogSaving();

            // Assert
            Assert.NotNull(Logs.LogFilePath);
            Assert.StartsWith(Program.DataDir, Logs.LogFilePath);

            string relativePath = Path.GetRelativePath(Program.DataDir, Logs.LogFilePath).Replace('\\', '/');
            DateTimeOffset time = DateTimeOffset.Now;
            DateTimeOffset time2 = time.AddMinutes(-1);

            // Allow for a minute rollover during execution
            string expectedPath1 = $"Logs/{time.Year:0000}-{time.Month:00}/{time.Day:00}-{time.Hour:00}-{time.Minute:00}.log";
            string expectedPath2 = $"Logs/{time2.Year:0000}-{time2.Month:00}/{time2.Day:00}-{time2.Hour:00}-{time2.Minute:00}.log";

            Assert.True(relativePath == expectedPath1 || relativePath == expectedPath2,
                $"Expected {expectedPath1} or {expectedPath2}, but got {relativePath}");
        }

        [Fact]
        public void StartLogSaving_Disabled_DoesNotSetLogFilePath()
        {
            // Arrange
            Program.ServerSettings.Logs.SaveLogToFile = false;
            Logs.LogFilePath = null; // reset just in case

            // Act
            Logs.StartLogSaving();

            // Assert
            Assert.Null(Logs.LogFilePath);
        }
    }
}
