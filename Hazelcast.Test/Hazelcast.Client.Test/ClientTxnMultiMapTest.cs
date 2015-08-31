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
        //internal const string name = "ClientTxnMultiMapTest";

        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public static void Destroy()
        {
        }


	    [Test]
	    public virtual void TestRemove()
	    {
            const string key = "key";
            const string value = "value";
	        var name = Name;
	        var multiMap = Client.GetMultiMap<string, string>(name);

	        multiMap.Put(key, value);
            ITransactionContext tx = Client.NewTransactionContext();

            tx.BeginTransaction();
            tx.GetMultiMap<string,string>(name).Remove(key,value);
            tx.CommitTransaction();

            Assert.AreEqual(new List<string>(), multiMap.Get(key));
	    }

	    [Test]
	    public virtual void TestRemoveAll()
	    {
            const string key = "key";
            const string value = "value";
	        var name = Name;
            var multiMap = Client.GetMultiMap<string, string>(name);
	        for (int i = 0; i < 10; i++)
	        {
	            multiMap.Put(key, value+i);
	        }
                

            ITransactionContext tx = Client.NewTransactionContext();

            tx.BeginTransaction();
            tx.GetMultiMap<string,string>(name).Remove(key);
            tx.CommitTransaction();

            Assert.AreEqual(new List<string>(), multiMap.Get(key));
	    }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestPutGetRemove()
        {
            var name = Name;
            var mm = Client.GetMultiMap<object, object>(name);

            for (int i = 0; i < 10; i++)
            {
                string key = i + "key";
                Client.GetMultiMap<object, object>(name).Put(key, "value");
                ITransactionContext context = Client.NewTransactionContext();
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
            var name = Name;
			var mm = Client.GetMultiMap<object,object>(name);
            string key = "key";
            Client.GetMultiMap<object, object>(name).Put(key, "value");
            ITransactionContext context = Client.NewTransactionContext();

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
