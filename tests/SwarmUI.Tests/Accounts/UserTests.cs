using Xunit;
using SwarmUI.Accounts;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System;
using SwarmUI.Core;

namespace SwarmUI.Tests.Accounts
{
    [Collection("Sequential")]
    public class UserTests : IDisposable
    {
        private string _originalDataDir;
        private string _testDataDir;

        public UserTests()
        {
            _originalDataDir = Program.DataDir;
            _testDataDir = $"Data_Test_{Guid.NewGuid()}";
            Program.DataDir = _testDataDir;
            if (!Directory.Exists(_testDataDir))
            {
                Directory.CreateDirectory(_testDataDir);
            }
            if (Program.ServerSettings == null)
            {
                Program.ServerSettings = new Settings();
            }
            // Create dummy Roles.fds to avoid File.Move exception in FDSUtility.SaveToFile
            File.WriteAllText($"{_testDataDir}/Roles.fds", "");
        }

        public void Dispose()
        {
            Program.DataDir = _originalDataDir;
            if (Directory.Exists(_testDataDir))
            {
                Directory.Delete(_testDataDir, true);
            }
        }

        private Session CreateMockSession(out SessionHandler sessionHandler)
        {
            // Set NoPersist before creating SessionHandler to avoid FDS exceptions
            bool wasNoPersist = Program.NoPersist;
            Program.NoPersist = true;
            try
            {
                sessionHandler = new SessionHandler();
                sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
                var user = new User(sessionHandler, new User.DatabaseEntry { ID = "test_user" });
                user.MayCreateSessions = true;
                return new Session() { User = user };
            }
            finally
            {
                Program.NoPersist = wasNoPersist;
            }
        }

        [Fact]
        public void SaveGenericData_WhenNoPersistIsTrue_DoesNotSave()
        {
            // Arrange
            bool wasNoPersist = Program.NoPersist;
            Program.NoPersist = true;
            try
            {
                var session = CreateMockSession(out var sessionHandler);
                var user = session.User;
                string dataname = "test_data_name";
                string name = "test_name";
                string data = "test_value";

                // Act
                user.SaveGenericData(dataname, name, data);

                // Assert
                var retrievedData = user.GetGenericData(dataname, name);
                Assert.Null(retrievedData);
            }
            finally
            {
                // Cleanup
                Program.NoPersist = wasNoPersist;
            }
        }

        [Fact]
        public void SaveGenericData_ValidInput_SavesSuccessfully()
        {
            // Arrange
            var session = CreateMockSession(out var sessionHandler);
            var user = session.User;
            string dataname = "test_data_name";
            string name = "test_name";
            string data = "test_value";

            // Act
            user.SaveGenericData(dataname, name, data);

            // Assert
            var retrievedData = user.GetGenericData(dataname, name);
            Assert.Equal(data, retrievedData);
        }

        [Fact]
        public void SaveGenericData_UpdateExisting_UpdatesSuccessfully()
        {
            // Arrange
            var session = CreateMockSession(out var sessionHandler);
            var user = session.User;
            string dataname = "test_data_name";
            string name = "test_name";
            string initialData = "test_value";
            string updatedData = "updated_value";

            user.SaveGenericData(dataname, name, initialData);

            // Act
            user.SaveGenericData(dataname, name, updatedData);

            // Assert
            var retrievedData = user.GetGenericData(dataname, name);
            Assert.Equal(updatedData, retrievedData);
        }
    }
}
