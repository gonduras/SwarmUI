using Xunit;
using SwarmUI.Utils;
using System;
using System.IO;
using System.Collections.Concurrent;

namespace SwarmUI.Tests.Utils
{
    [Collection("Sequential")]
    public class LogsTests : IDisposable
    {
        private readonly string _originalLogFilePath;
        private readonly ConcurrentQueue<string> _originalLogsToSave;
        private readonly string _tempLogFilePath;

        public LogsTests()
        {
            // Snapshot global static state
            _originalLogFilePath = Logs.LogFilePath;
            _originalLogsToSave = Logs.LogsToSave;

            // Setup isolated test state
            _tempLogFilePath = Path.GetTempFileName();
            Logs.LogFilePath = _tempLogFilePath;
            Logs.LogsToSave = new ConcurrentQueue<string>();
        }

        public void Dispose()
        {
            // Restore global static state
            Logs.LogFilePath = _originalLogFilePath;
            Logs.LogsToSave = _originalLogsToSave;

            // Clean up isolated test resources
            if (File.Exists(_tempLogFilePath))
            {
                File.Delete(_tempLogFilePath);
            }
        }

        [Fact]
        public void SaveLogsToFileOnce_EmptyQueue_DoesNothing()
        {
            // Arrange
            File.WriteAllText(_tempLogFilePath, "Initial content");

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            var content = File.ReadAllText(_tempLogFilePath);
            Assert.Equal("Initial content", content); // File is unchanged
            Assert.Empty(Logs.LogsToSave);
        }

        [Fact]
        public void SaveLogsToFileOnce_HasLogs_AppendsToFileAndClearsQueue()
        {
            // Arrange
            File.WriteAllText(_tempLogFilePath, "Initial content\n");
            Logs.LogsToSave.Enqueue("Test log 1");
            Logs.LogsToSave.Enqueue("Test log 2");

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            var content = File.ReadAllText(_tempLogFilePath);
            Assert.Equal("Initial content\nTest log 1\nTest log 2\n", content);
            Assert.Empty(Logs.LogsToSave); // Queue should be empty after dequeuing
        }
    }
}
