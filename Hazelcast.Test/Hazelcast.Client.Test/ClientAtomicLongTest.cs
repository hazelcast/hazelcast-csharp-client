using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientAtomicLongTest:HazelcastBaseTest
	{
        internal const string name = "ClientAtomicLongTest";

		internal static IAtomicLong l;
        //
        [SetUp]
        public void Init()
        {
            l = client.GetAtomicLong(Name);
            l.Set(0);
        }

        [TearDown]
        public static void Destroy()
        {
            l.Destroy();
        }

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void Test()
		{
			Assert.AreEqual(0, l.GetAndAdd(2));
			Assert.AreEqual(2, l.Get());
			l.Set(5);
			Assert.AreEqual(5, l.Get());
			Assert.AreEqual(8, l.AddAndGet(3));
			Assert.IsFalse(l.CompareAndSet(7, 4));
			Assert.AreEqual(8, l.Get());
			Assert.IsTrue(l.CompareAndSet(8, 4));
			Assert.AreEqual(4, l.Get());
			Assert.AreEqual(3, l.DecrementAndGet());
			Assert.AreEqual(3, l.GetAndIncrement());
			Assert.AreEqual(4, l.GetAndSet(9));
			Assert.AreEqual(10, l.IncrementAndGet());
		}
	}
}
