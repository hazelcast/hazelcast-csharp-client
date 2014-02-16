using System;
using System.Threading;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTxnMapTest:HazelcastBaseTest
	{
        //internal const string name = "ClientTxnMapTest";

        internal static IMap<object, object> map;
	    internal string name="test";

        [SetUp]
        public void Init()
        {
            //name = Name;
            map = client.GetMap<object, object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
            map.Destroy();
            //client.GetLifecycleService().Shutdown();
        }


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutGet()
		{
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalMap<object, object> txnMap = context.GetMap<object, object>(name);
			Assert.IsNull(txnMap.Put("key1", "value1"));
			Assert.AreEqual("value1", txnMap.Get("key1"));
            Assert.IsNull(map.Get("key1"));
			context.CommitTransaction();
            Assert.AreEqual("value1", map.Get("key1"));
		}


		[Test]
        public void testPutWithTTL()
        {
            ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalMap<object, object> txnMap = context.GetMap<object, object>(name);
			Assert.IsNull(txnMap.Put("key1", "value1",5,TimeUnit.SECONDS));
			Assert.AreEqual("value1", txnMap.Get("key1"));
			context.CommitTransaction();
        
			Assert.AreEqual("value1", map.Get("key1"));
            Thread.Sleep(10000);
		    Assert.AreEqual(0,map.Size());
            Assert.IsNull(map.Get("key1"));
        }


	    [Test]
	    public void testGetForUpdate()
	    {

            ITransactionContext context = client.NewTransactionContext();
            context.BeginTransaction();
            ITransactionalMap<object, object> txnMap = context.GetMap<object, object>(name);
            txnMap.Put("key1", "value1");
            Assert.AreEqual("value1", txnMap.Get("key1"));
	        Assert.AreEqual("value1", txnMap.GetForUpdate("key1"));

	        Assert.IsTrue(map.IsLocked("key1"));

            context.CommitTransaction();

            Assert.IsFalse(map.IsLocked("key1"));
            Assert.AreEqual("value1", map.Get("key1"));

	    }

        ///// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        //[Test]
        //public virtual void TestGetForUpdate()
        //{
        //    //var map = client.GetMap<string, int>(name);
        //    CountdownEvent latch1 = new CountdownEvent(1);
        //    CountdownEvent latch2 = new CountdownEvent(1);
        //    map.Put("var", 0);
        //    AtomicBoolean pass = new AtomicBoolean(true);


        //    var t1 = new Thread(delegate(object o)
        //    {
        //        try
        //        {
        //            latch1.Wait(TimeSpan.FromSeconds(100));
        //            pass.Set(map.TryPut("var", 1, 0, TimeUnit.SECONDS) == false);
        //            latch2.Signal();
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    });
        //    t1.Start();

        //    bool b = client.ExecuteTransaction(new _TransactionalTask(latch1, latch2,name));
        //    Assert.IsTrue(b);
        //    Assert.IsTrue(pass.Get());
        //    Assert.IsTrue(map.TryPut("var", 1, 0, TimeUnit.SECONDS));
        //}


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestKeySetValues()
		{
            //var map = client.GetMap<object, object>(name);
			map.Put("key1", "value1");
			map.Put("key2", "value2");
            ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalMap<object, object> txMap = context.GetMap<object, object>(name);
			Assert.IsNull(txMap.Put("key3", "value3"));
			Assert.AreEqual(3, txMap.Size());
			Assert.AreEqual(3, txMap.KeySet().Count);
			Assert.AreEqual(3, txMap.Values().Count);
			context.CommitTransaction();
			Assert.AreEqual(3, map.Size());
			Assert.AreEqual(3, map.KeySet().Count);
			Assert.AreEqual(3, map.Values().Count);
		}
	}
}
