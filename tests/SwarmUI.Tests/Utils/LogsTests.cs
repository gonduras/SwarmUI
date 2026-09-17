using System;
using System.IO;
using System.Collections.Concurrent;
using Xunit;
using SwarmUI.Utils;

namespace SwarmUI.Tests.Utils
{
    public class LogsTests : IDisposable
    {
        private string originalLogFilePath;
        private ConcurrentQueue<string> originalLogsToSave;
        private string testLogFilePath;

        public LogsTests()
        {
            // Store original state
            originalLogFilePath = Logs.LogFilePath;
            originalLogsToSave = Logs.LogsToSave;

            // Setup test state
            testLogFilePath = Path.Combine(Path.GetTempPath(), $"test_logs_{Guid.NewGuid()}.txt");
            Logs.LogFilePath = testLogFilePath;
            Logs.LogsToSave = new ConcurrentQueue<string>();
        }

        public void Dispose()
        {
            // Restore original state
            Logs.LogFilePath = originalLogFilePath;
            Logs.LogsToSave = originalLogsToSave;

            // Cleanup test file
            if (File.Exists(testLogFilePath))
            {
                File.Delete(testLogFilePath);
            }
        }

        [Fact]
        public void SaveLogsToFileOnce_WhenQueueIsEmpty_DoesNotThrowOrWriteFile()
        {
            // Arrange
            Assert.True(Logs.LogsToSave.IsEmpty);

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            Assert.False(File.Exists(Logs.LogFilePath));
        }

        [Fact]
        public void SaveLogsToFileOnce_WhenQueueHasItems_WritesToFile()
        {
            // Arrange
            string logLine1 = "Test log line 1";
            string logLine2 = "Test log line 2";
            Logs.LogsToSave.Enqueue(logLine1);
            Logs.LogsToSave.Enqueue(logLine2);

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            Assert.True(File.Exists(Logs.LogFilePath));
            string fileContent = File.ReadAllText(Logs.LogFilePath);
            Assert.Contains(logLine1, fileContent);
            Assert.Contains(logLine2, fileContent);

            // Queue should be empty after writing
            Assert.True(Logs.LogsToSave.IsEmpty);
        }
    }
}
