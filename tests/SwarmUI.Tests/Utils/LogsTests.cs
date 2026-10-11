using Xunit;
using SwarmUI.Utils;
using System.IO;
using System.Collections.Concurrent;
using System;

namespace SwarmUI.Tests.Utils
{
    [Collection("Sequential")]
    public class LogsTests : IDisposable
    {
        private readonly string _testLogFilePath;

        public LogsTests()
        {
            _testLogFilePath = Path.Combine(Path.GetTempPath(), $"SwarmUI_TestLogs_{Guid.NewGuid()}.txt");
            Logs.LogFilePath = _testLogFilePath;
            Logs.LogsToSave = new ConcurrentQueue<string>();
        }

        public void Dispose()
        {
            if (File.Exists(_testLogFilePath))
            {
                File.Delete(_testLogFilePath);
            }
        }

        [Fact]
        public void SaveLogsToFileOnce_WithEmptyQueue_DoesNotCreateFile()
        {
            // Arrange
            Logs.LogsToSave.Clear();

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            Assert.False(File.Exists(_testLogFilePath));
        }

        [Fact]
        public void SaveLogsToFileOnce_WithItemsInQueue_AppendsToFile()
        {
            // Arrange
            Logs.LogsToSave.Enqueue("Test log 1");
            Logs.LogsToSave.Enqueue("Test log 2");

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            Assert.True(File.Exists(_testLogFilePath));
            var lines = File.ReadAllLines(_testLogFilePath);
            Assert.Equal(2, lines.Length);
            Assert.Equal("Test log 1", lines[0]);
            Assert.Equal("Test log 2", lines[1]);
            Assert.Empty(Logs.LogsToSave);
        }

        [Fact]
        public void SaveLogsToFileOnce_CalledMultipleTimes_AppendsCorrectly()
        {
            // Arrange
            Logs.LogsToSave.Enqueue("First batch");
            Logs.SaveLogsToFileOnce();

            Logs.LogsToSave.Enqueue("Second batch");

            // Act
            Logs.SaveLogsToFileOnce();

            // Assert
            Assert.True(File.Exists(_testLogFilePath));
            var lines = File.ReadAllLines(_testLogFilePath);
            Assert.Equal(2, lines.Length);
            Assert.Equal("First batch", lines[0]);
            Assert.Equal("Second batch", lines[1]);
        }
    }
}
