using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientIdGeneratorTest:HazelcastBaseTest
	{
        internal const string name = "ClientIdGeneratorTest";

		internal static IIdGenerator i;

        [SetUp]
        public void Init()
        {
            i = client.GetIdGenerator(Name);
        }

        [TearDown]
        public static void Destroy()
        {
        }
		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestGenerator()
		{
			Assert.IsTrue(i.Init(3569));
			Assert.IsFalse(i.Init(4569));
			Assert.AreEqual(3570, i.NewId());
		}
	}
}
