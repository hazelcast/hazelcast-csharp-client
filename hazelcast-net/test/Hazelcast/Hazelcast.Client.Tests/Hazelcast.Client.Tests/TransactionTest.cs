using System;
using System.Threading;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class TransactionTest :HazelcastTest
	{
		[Test]
    public void rollbackTransactionMap() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IMap<String, String> map = hClient.getMap<String, String>("rollbackTransactionMap");
        map.put("1", "A");
        Assert.AreEqual("A", map.get("1"));
        transaction.rollback();
        Assert.IsNull(map.get("1"));
    }

    [Test]
    public void commitTransactionMap() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IMap<String, String> map = hClient.getMap<String, String>("commitTransactionMap");
        map.put("1", "A");
        Assert.AreEqual("A", map.get("1"));
        transaction.commit();
        Assert.AreEqual("A", map.get("1"));
    }

    [Test]
    public void testTransactionVisibilityFromDifferentThreads(){
        HazelcastInstance hClient = getHazelcastClient();
        CountdownEvent latch = new CountdownEvent(1);
		CountdownEvent notify = new CountdownEvent(1);
        Object o = new Object();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IMap<String, String> map = hClient.getMap<String, String>("testTransactionVisibilityFromDifferentThreads");
        map.put("1", "A");
        Assert.AreEqual("A", map.get("1"));
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				Assert.IsNull(map.get("1"));
                if (!map.containsKey("1")) {
                    latch.Signal();
                }
                
                notify.Signal();
                
			});
			
		
        notify.Wait();
        transaction.rollback();
        Assert.IsNull(map.get("1"));
        Assert.IsTrue(latch.Wait(1));
    }

    [Test]
    public void rollbackTransactionList() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IList<String> list = hClient.getList<String>("rollbackTransactionList");
        list.Add("Istanbul");
        transaction.rollback();
        Assert.IsTrue(list.Count==0);
    }

    [Test]
    public void commitTransactionList() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IList<String> list = hClient.getList<String>("commitTransactionList");
        list.Add("Istanbul");
        transaction.commit();
        Assert.IsTrue(list.Contains("Istanbul"));
    }

    [Test]
    public void rollbackTransactionSet() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.ISet<String> set = hClient.getSet<String>("rollbackTransactionSet");
        set.Add("Istanbul");
        transaction.rollback();
        Assert.IsTrue(set.Count==0);
    }

    [Test]
    public void commitTransactionSet() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.ISet<String> set = hClient.getSet<String>("commitTransactionSet");
        set.Add("Istanbul");
        transaction.commit();
        //Assert.IsTrue(set.Contains("Istanbul"));
    }

    [Test]
    public void rollbackTransactionQueue() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IQueue<String> q = hClient.getQueue<String>("rollbackTransactionQueue");
        q.offer("Istanbul");
        transaction.rollback();
        Assert.IsTrue(q.Count == 0);
    }

    [Test]
    public void commitTransactionQueue() {
        HazelcastInstance hClient = getHazelcastClient();
        Transaction transaction = hClient.getTransaction();
        transaction.begin();
        Hazelcast.Core.IQueue<String> q = hClient.getQueue<String>("commitTransactionQueue");
        q.offer("Istanbul");
        transaction.commit();
        Assert.AreEqual("Istanbul", q.poll());
    }
	}
}

