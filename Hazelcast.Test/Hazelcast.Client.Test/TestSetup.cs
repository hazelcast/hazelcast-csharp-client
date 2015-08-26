using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [SetUpFixture]
    public class TestSetup
    {
        public static HazelcastNode Node { get; private set;  }
        public static IHazelcastInstance Client { get; private set; }

        [SetUp]
        public void Setup()
        {
            Node = new HazelcastNode();
            Node.Start();
            Client = new HazelcastTestClient().Init();
        }

        [TearDown]
        public void TearDown()
        {
            Client.Shutdown();
            Node.Stop();
        }

    }
}