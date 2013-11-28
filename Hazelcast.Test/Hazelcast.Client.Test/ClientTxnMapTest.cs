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
        internal const string name = "ClientTxnMapTest";

        //internal static IHazelcastMap<object, object> map;

        [SetUp]
        public static void Init()
        {
            InitClient();
            //map = client.GetMap<object, object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            //map.Clear();
            //client.GetLifecycleService().Shutdown();
        }


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutGet()
		{
			string name = "defMap";
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalMap<object, object> map = context.GetMap<object, object>(name);
			Assert.IsNull(map.Put("key1", "value1"));
			Assert.AreEqual("value1", map.Get("key1"));
            Assert.IsNull(client.GetMap<object, object>(name).Get("key1"));
			context.CommitTransaction();
            Assert.AreEqual("value1", client.GetMap<object, object>(name).Get("key1"));
		}

		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		[Test]
		public virtual void TestGetForUpdate()
		{
            var map = client.GetMap<string, int>("testTxnGetForUpdate");
            CountdownEvent latch1 = new CountdownEvent(1);
            CountdownEvent latch2 = new CountdownEvent(1);
			map.Put("var", 0);
			AtomicBoolean pass = new AtomicBoolean(true);


		    var t1 = new Thread(delegate(object o)
		    {
                try
                {
                    latch1.Wait(TimeSpan.FromSeconds(100));
                    pass.Set(map.TryPut("var", 1, 0, TimeUnit.SECONDS) == false);
                    latch2.Signal();
                }
                catch (Exception)
                {
                }
            });
            t1.Start();

            bool b = client.ExecuteTransaction(new _TransactionalTask(latch1, latch2));
			Assert.IsTrue(b);
			Assert.IsTrue(pass.Get());
			Assert.IsTrue(map.TryPut("var", 1, 0, TimeUnit.SECONDS));
		}


		private sealed class _TransactionalTask : ITransactionalTask<bool>
		{
            public _TransactionalTask(CountdownEvent latch1, CountdownEvent latch2)
			{
				this.latch1 = latch1;
				this.latch2 = latch2;
			}

			/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
			public bool Execute(ITransactionalTaskContext context)
			{
				try
				{
                    ITransactionalMap<string, int> txMap = context.GetMap<string, int>("testTxnGetForUpdate");
					txMap.GetForUpdate("var");
					latch1.Signal();
                    latch2.Wait(TimeSpan.FromSeconds(100));
				}
				catch (Exception)
				{
				}
				return true;
			}

            private readonly CountdownEvent latch1;

            private readonly CountdownEvent latch2;
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestKeySetValues()
		{
			string name = "testKeySetValues";
            var map = client.GetMap<object, object>(name);
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
			Assert.AreEqual(3, map.Keys().Count);
			Assert.AreEqual(3, map.Values().Count);
		}
	}
}
