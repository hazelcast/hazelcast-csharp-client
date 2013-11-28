using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTxnListTest:HazelcastBaseTest
	{
        internal const string name = "ClientTxnListTest";

        internal static IHazelcastList<object> list;

        [SetUp]
        public static void Init()
        {
            InitClient();
            list = client.GetList<object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            list.Clear();
            //client.GetLifecycleService().Shutdown();
        }

		/// <exception cref="System.Exception"></exception>
		[NUnit.Framework.Test]
		public virtual void TestAddRemove()
		{
			list.Add("item1");
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
			ITransactionalList<object> listTx = context.GetList<object>(name);
            NUnit.Framework.Assert.IsTrue(listTx.Add("item2"));
            NUnit.Framework.Assert.AreEqual(2, listTx.Size());
            NUnit.Framework.Assert.AreEqual(1, list.Count);
            NUnit.Framework.Assert.IsFalse(listTx.Remove("item3"));
            NUnit.Framework.Assert.IsTrue(listTx.Remove("item1"));
			context.CommitTransaction();
            NUnit.Framework.Assert.AreEqual(1, list.Count);
		}
	}
}
