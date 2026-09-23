using Xunit;
using SwarmUI.Accounts;
using SwarmUI.Core;
using System.IO;
using System.Collections.Concurrent;
using System;

namespace SwarmUI.Tests.Accounts
{
    public class UserTests : IDisposable
    {
        private string TestDataDir;
        private SessionHandler SessionHandlerSource;
        private User TestUser;
        private object Lock = new object();

        public UserTests()
        {
            // Ensure isolated Data directory for this test suite
            TestDataDir = $"Data_UserTests_{Guid.NewGuid()}";
            if (!Directory.Exists(TestDataDir))
            {
                Directory.CreateDirectory(TestDataDir);
            }

            // Bypass full SessionHandler initialization to avoid database issues
            // This is mentioned in memory: "To properly mock a User for tests without triggering LiteDB database initialization issues, you can bypass full SessionHandler initialization by passing null: new User(null, new User.DatabaseEntry { ID = "test_user" })."

            // However, we *need* a session handler and DB for generic data methods!
            // Let's use the memory hint:
            Program.NoPersist = false; // Need persist to test saving!
            Program.DataDir = TestDataDir;

            // We need to bypass the failure in `ApplyDefaultPermissions()` because `Program.ServerSettings` is null, or `role.Data.Save(true)` fails.
            // Oh wait, `role.Data` is an `AutoConfiguration`. It might crash if `Program.ServerSettings` is null, or we just bypass SessionHandler entirely?
            // Actually, we can use a simpler setup. We can manually create a SessionHandler and avoid some pitfalls, or mock it?

            // Let's create a minimal session handler
            // Wait, the failure was `System.IndexOutOfRangeException` at `FreneticDataSyntax.AutoConfiguration.Save(Boolean includeUnmodified)`.
            // Why? Because `role.Data.Save(true)` crashes.

            SessionHandlerSource = new SessionHandler();
            SessionHandlerSource.Roles = new ConcurrentDictionary<string, Role>(); // Ensure no roles throw exceptions on save
            TestUser = new User(SessionHandlerSource, new User.DatabaseEntry { ID = "test_user" });
        }

        public void Dispose()
        {
            // Cleanup database connection
            SessionHandlerSource?.Database?.Dispose();

            // Restore defaults if needed (though other tests might interfere if run in parallel)
            Program.NoPersist = false;

            // Cleanup isolated test directory
            if (Directory.Exists(TestDataDir))
            {
                try
                {
                    Directory.Delete(TestDataDir, true);
                }
                catch { } // Ignore cleanup errors if files are locked
            }
        }

        [Fact]
        public void SaveGenericData_WithValidData_SavesToDatabase()
        {
            // Arrange
            string dataname = "testdata";
            string name = "mykey";
            string data = "myvalue";

            // Act
            TestUser.SaveGenericData(dataname, name, data);

            // Assert
            string result = TestUser.GetGenericData(dataname, name);
            Assert.Equal(data, result);
        }

        [Fact]
        public void SaveGenericData_WhenNoPersist_DoesNotSave()
        {
            // Arrange
            Program.NoPersist = true;
            string dataname = "testdata_nopersist";
            string name = "mykey";
            string data = "myvalue";

            try
            {
                // Act
                TestUser.SaveGenericData(dataname, name, data);

                // Assert
                string result = TestUser.GetGenericData(dataname, name);
                Assert.Null(result); // Should be null because it shouldn't have been saved
            }
            finally
            {
                // Ensure NoPersist is reset even if test fails
                Program.NoPersist = false;
            }
        }

        [Fact]
        public void SaveGenericData_UpdateExisting_UpdatesCorrectly()
        {
            // Arrange
            string dataname = "testdata_update";
            string name = "mykey";
            string data1 = "myvalue1";
            string data2 = "myvalue2";

            // Act
            TestUser.SaveGenericData(dataname, name, data1);
            TestUser.SaveGenericData(dataname, name, data2);

            // Assert
            string result = TestUser.GetGenericData(dataname, name);
            Assert.Equal(data2, result);
        }

        [Fact]
        public void DeleteGenericData_WorksCorrectly()
        {
            // Arrange
            string dataname = "testdata_delete";
            string name = "mykey";
            string data = "myvalue";
            TestUser.SaveGenericData(dataname, name, data);

            // Verify it was saved
            Assert.Equal(data, TestUser.GetGenericData(dataname, name));

            // Act
            bool deleted = TestUser.DeleteGenericData(dataname, name);

            // Assert
            Assert.True(deleted);
            string result = TestUser.GetGenericData(dataname, name);
            Assert.Null(result);
        }
    }
}
