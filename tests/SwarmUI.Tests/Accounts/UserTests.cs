using System;
using System.Collections.Concurrent;
using System.IO;
using Xunit;
using LiteDB;
using SwarmUI.Accounts;
using SwarmUI.Core;
using System.Runtime.CompilerServices;

namespace SwarmUI.Tests.Accounts
{
    [Collection("Sequential")]
    public class UserTests : IDisposable
    {
        private SessionHandler _sessionHandler;

        public UserTests()
        {
            if (Program.ServerSettings == null)
            {
                Program.ServerSettings = new Settings();
            }

            _sessionHandler = new SessionHandler();
            // Assign roles directly to avoid null ref in BuildRoles
            _sessionHandler.Roles = new ConcurrentDictionary<string, Role>();
            // Swap out the database for an in-memory database to prevent pollution
            _sessionHandler.Database = new LiteDatabase("Filename=:memory:;Mode=Exclusive");
            _sessionHandler.GenericData = _sessionHandler.Database.GetCollection<SessionHandler.GenericDataStore>("generic_data");
        }

        public void Dispose()
        {
            _sessionHandler?.Database?.Dispose();
        }

        // We bypass the constructor of User to avoid the NullReferenceException during Initialization.
        // It relies on Program.ServerSettings.DefaultUser and other complex things.
        // Since we are only testing GenericData methods, we only need SessionHandlerSource, UserID, and Data
        private User CreateMockUser()
        {
            User user = (User)RuntimeHelpers.GetUninitializedObject(typeof(User));
            user.Data = new User.DatabaseEntry { ID = "test_user_data" };
            user.SessionHandlerSource = _sessionHandler;
            user.MayCreateSessions = true; // Required for SaveGenericData to proceed
            return user;
        }

        [Fact]
        public void GenericData_SaveAndRetrieve_ReturnsSavedData()
        {
            // Arrange
            var user = CreateMockUser();
            string dataname = "test_category";
            string name = "test_item";
            string data = "test_value";

            bool prev = Program.NoPersist;
            Program.NoPersist = false;
            try
            {
                // Act
                user.SaveGenericData(dataname, name, data);
                string retrievedData = user.GetGenericData(dataname, name);

                // Assert
                Assert.Equal(data, retrievedData);
            }
            finally
            {
                Program.NoPersist = prev;
            }
        }

        [Fact]
        public void GenericData_GetNonExistent_ReturnsNull()
        {
            // Arrange
            var user = CreateMockUser();
            string dataname = "test_category";
            string name = "nonexistent_item";

            bool prev = Program.NoPersist;
            Program.NoPersist = false;
            try
            {
                // Act
                string retrievedData = user.GetGenericData(dataname, name);

                // Assert
                Assert.Null(retrievedData);
            }
            finally
            {
                Program.NoPersist = prev;
            }
        }

        [Fact]
        public void GenericData_ListAllGenericData_ReturnsSavedNames()
        {
            // Arrange
            var user = CreateMockUser();
            string dataname = "test_list_category";

            bool prev = Program.NoPersist;
            Program.NoPersist = false;
            try
            {
                // Act
                user.SaveGenericData(dataname, "item1", "value1");
                user.SaveGenericData(dataname, "item2", "value2");
                var items = user.ListAllGenericData(dataname);

                // Assert
                Assert.Contains("item1", items);
                Assert.Contains("item2", items);
                Assert.Equal(2, items.Count);
            }
            finally
            {
                Program.NoPersist = prev;
            }
        }

        [Fact]
        public void GenericData_DeleteGenericData_RemovesData()
        {
            // Arrange
            var user = CreateMockUser();
            string dataname = "test_delete_category";
            string name = "item_to_delete";

            bool prev = Program.NoPersist;
            Program.NoPersist = false;
            try
            {
                user.SaveGenericData(dataname, name, "value");

                // Act
                bool deleted = user.DeleteGenericData(dataname, name);
                string retrievedData = user.GetGenericData(dataname, name);

                // Assert
                Assert.True(deleted);
                Assert.Null(retrievedData);
            }
            finally
            {
                Program.NoPersist = prev;
            }
        }
    }
}
