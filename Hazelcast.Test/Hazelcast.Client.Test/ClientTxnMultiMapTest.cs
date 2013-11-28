using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTxnMultiMapTest:HazelcastBaseTest
	{
        internal const string name = "ClientTxnMultiMapTest";

        [SetUp]
        public static void Init()
        {
            InitClient();
        }

        [TearDown]
        public static void Destroy()
        {
        }


        /// <exception cref="System.Exception"></exception>
        [Test,Ignore]
        public virtual void TestPutGetRemove()
        {
            var mm = client.GetMultiMap<object, object>(name);

            for (int i = 0; i < 10; i++)
            {
                string key = i + "key";
                client.GetMultiMap<object, object>(name).Put(key, "value");
                ITransactionContext context = client.NewTransactionContext();
                context.BeginTransaction();
                var multiMap = context.GetMultiMap<object, object>(name);
                Assert.IsFalse(multiMap.Put(key, "value"));
                Assert.IsTrue(multiMap.Put(key, "value1"));
                Assert.IsTrue(multiMap.Put(key, "value2"));
                Assert.AreEqual(3, multiMap.Get(key).Count);
                context.CommitTransaction();
                Assert.AreEqual(3, mm.Get(key).Count);
            }
            
        }

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutGetRemove2()
		{
			var mm = client.GetMultiMap<object,object>(name);
            string key = "key";
            client.GetMultiMap<object, object>(name).Put(key, "value");
            ITransactionContext context = client.NewTransactionContext();

            context.BeginTransaction();

            var multiMap = context.GetMultiMap<object, object>(name);
            
            Assert.IsFalse(multiMap.Put(key, "value"));
            Assert.IsTrue(multiMap.Put(key, "value1"));
            Assert.IsTrue(multiMap.Put(key, "value2"));
            Assert.AreEqual(3, multiMap.Get(key).Count);
            
            context.CommitTransaction();
            
            Assert.AreEqual(3, mm.Get(key).Count);

		}


	}
}
