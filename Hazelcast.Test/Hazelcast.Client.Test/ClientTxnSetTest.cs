using System.Collections.Generic;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[NUnit.Framework.TestFixture]
	public class ClientTxnSetTest:HazelcastBaseTest
	{
        //internal const string name = "test";

        [SetUp]
        public void Init()
        {
            //map = client.GetMap<object, object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            //map.Clear();
            //client.GetLifecycleService().Shutdown();
        }
		/// <exception cref="System.Exception"></exception>
		[NUnit.Framework.Test]
		public virtual void TestAddRemove()
		{
		    var name = Name;
            var s = client.GetSet<object>(name);
			s.Add("item1");
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalSet<object> set = context.GetSet<object>(name);
			NUnit.Framework.Assert.IsTrue(set.Add("item2"));
			NUnit.Framework.Assert.AreEqual(2, set.Size());
			NUnit.Framework.Assert.AreEqual(1, s.Count);
			NUnit.Framework.Assert.IsFalse(set.Remove("item3"));
			NUnit.Framework.Assert.IsTrue(set.Remove("item1"));
			context.CommitTransaction();
			NUnit.Framework.Assert.AreEqual(1, s.Count);
		}
	}
}
