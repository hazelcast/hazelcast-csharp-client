using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class RandomProviderExtensions
    {
        [Test]
        public void Next()
        {
            var z = RandomProvider.Random.Next();
        }
    }
}
