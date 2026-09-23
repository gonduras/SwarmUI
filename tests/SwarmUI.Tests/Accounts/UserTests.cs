using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Xunit;
using SwarmUI.Core;
using SwarmUI.Accounts;
using SwarmUI.Utils;
using FreneticUtilities.FreneticExtensions;

namespace SwarmUI.Tests.Accounts
{
    [Collection("Sequential")]
    public class UserTests : IDisposable
    {
        private readonly string _tempDataDir;
        private readonly SessionHandler _sessionHandler;

        public UserTests()
        {
            _tempDataDir = Path.Combine(Path.GetTempPath(), "SwarmUI_Test_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataDir);

            Program.DataDir = _tempDataDir;
            Program.ServerSettings = new Settings();
            Program.NoPersist = false;

            _sessionHandler = new SessionHandler();
            _sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
        }

        public void Dispose()
        {
            _sessionHandler.Shutdown();
            if (Directory.Exists(_tempDataDir))
            {
                Directory.Delete(_tempDataDir, true);
            }
        }

        [Fact]
        public void SaveGenericData_Success()
        {
            // Arrange
            User user = _sessionHandler.GetUser("test_user_save");
            string dataname = "test_data";
            string name = "test_item";
            string data = "test_value";

            // Act
            user.SaveGenericData(dataname, name, data);

            // Assert
            var retrievedData = user.GetAllGenericData(dataname);
            Assert.NotEmpty(retrievedData);
            Assert.Contains(retrievedData, x => x.Data == data);
        }

        [Fact]
        public void SaveGenericData_NoPersist_ReturnsEarly()
        {
            // Arrange
            Program.NoPersist = true;
            User user = _sessionHandler.GetUser("test_user_nopersist");
            string dataname = "test_data_nopersist";
            string name = "test_item";
            string data = "test_value";

            // Act
            user.SaveGenericData(dataname, name, data);

            // Assert
            var retrievedData = user.GetAllGenericData(dataname);
            Assert.Empty(retrievedData);

            // Reset for other tests
            Program.NoPersist = false;
        }

        [Fact]
        public void SaveGenericData_UpdateExisting_Success()
        {
             // Arrange
            User user = _sessionHandler.GetUser("test_user_update");
            string dataname = "test_data_update";
            string name = "test_item";
            string data1 = "test_value1";
            string data2 = "test_value2";

            // Act
            user.SaveGenericData(dataname, name, data1);
            user.SaveGenericData(dataname, name, data2);

            // Assert
            var retrievedData = user.GetAllGenericData(dataname);
            Assert.Single(retrievedData);
            Assert.Equal(data2, retrievedData[0].Data);
        }

        [Fact]
        public void DeleteGenericData_Success()
        {
            // Arrange
            User user = _sessionHandler.GetUser("test_user_delete");
            string dataname = "test_data_delete";
            string name = "test_item";
            string data = "test_value";
            user.SaveGenericData(dataname, name, data);

            // Act
            bool result = user.DeleteGenericData(dataname, name);

            // Assert
            Assert.True(result);
            var retrievedData = user.GetAllGenericData(dataname);
            Assert.Empty(retrievedData);
        }
    }
}
