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
            i = Client.GetIdGenerator(Name);
        }

        [TearDown]
        public static void Destroy()
        {
            i.Destroy();
        }
		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestGenerator()
		{
			Assert.IsFalse(i.Init(-4569));

			Assert.IsTrue(i.Init(3569));
			Assert.IsFalse(i.Init(4569));
			Assert.AreEqual(3570, i.NewId());
		}
        
        [Test]
		public virtual void TestGeneratorBlockSize()
        {
            for (int j = 0; j < 10000; j++)
            {
                i.NewId();
            }

            Assert.AreEqual(10000, i.NewId());
            Assert.AreEqual(10001, i.NewId());
        }
	}
}
