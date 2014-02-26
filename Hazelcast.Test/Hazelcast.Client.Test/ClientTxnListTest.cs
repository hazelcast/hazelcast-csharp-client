using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTxnListTest:HazelcastBaseTest
	{
        //internal const string name2 = "ClientTxnListTest";

        internal static IHList<object> list;

        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public static void Destroy()
        {
            list.Clear();
            list.Destroy();
        }

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAddRemove()
		{
		    var name = Name;
            list = client.GetList<object>(name);
			list.Add("item1");
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
			ITransactionalList<object> listTx = context.GetList<object>(name);
            Assert.IsTrue(listTx.Add("item2"));
            Assert.AreEqual(2, listTx.Size());
            Assert.AreEqual(1, list.Count);
            Assert.IsFalse(listTx.Remove("item3"));
            Assert.IsTrue(listTx.Remove("item1"));
			context.CommitTransaction();
            Assert.AreEqual(1, list.Count);
            listTx.Destroy();
		}
	}
}
