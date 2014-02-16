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
        }

		/// <exception cref="System.Exception"></exception>
		[NUnit.Framework.Test]
		public virtual void TestAddRemove()
		{
		    var name = Name;
            list = client.GetList<object>(name);
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
