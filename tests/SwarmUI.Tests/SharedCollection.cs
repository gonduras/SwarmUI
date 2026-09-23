using Xunit;

namespace SwarmUI.Tests
{
    [CollectionDefinition("Sequential")]
    public class SequentialCollection : ICollectionFixture<object>
    {
    }
}
