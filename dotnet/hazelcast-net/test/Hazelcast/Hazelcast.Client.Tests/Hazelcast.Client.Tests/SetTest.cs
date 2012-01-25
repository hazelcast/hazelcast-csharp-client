using System;
using NUnit.Framework;
using System.Threading;
using Hazelcast.Core;
using System.Collections.Generic;
namespace Hazelcast.Client.Tests 
{
	[TestFixture()]
	public class SetTest : HazelcastTest
	{
		[Test()]
		public void TestCase ()
		{
		}
		
	[Test]
    public void getSetName() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<String> set = hClient.getSet<String>("getSetName");
        Assert.AreEqual("getSetName", set.getName());
			set.destroy();
    }

    [Test]
	[Ignore]
    public void addRemoveItemListener()  {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<String> set = hClient.getSet<String>("addRemoveItemListenerSet");
        CountdownEvent addLatch = new CountdownEvent(2);
        CountdownEvent removeLatch = new CountdownEvent(2);
        ItemListener<String> listener = new MyItemListener<String>(addLatch, removeLatch);
        set.addItemListener(listener, true);
        set.Add("hello");
        set.Add("hello");
        set.Remove("hello");
        set.Remove("hello");
        while (removeLatch.CurrentCount != 1) {
            Thread.Sleep(50);
        }
        set.removeItemListener(listener);
        set.Add("hello");
        set.Add("hello");
        set.Remove("hello");
        set.Remove("hello");
        Thread.Sleep(50);
        Assert.AreEqual(1, addLatch.CurrentCount);
        Assert.AreEqual(1, removeLatch.CurrentCount);
			set.destroy();
	}
		
	class MyItemListener<String> :ItemListener<String>{
		CountdownEvent addLatch;
		CountdownEvent removeLatch;
				
		public MyItemListener(CountdownEvent addLatch, CountdownEvent removeLatch){
			this.addLatch = addLatch;
			this.removeLatch = removeLatch;
		}	
		
		public void itemAdded<String>(ItemEvent<String> itemEvent) {
		    Assert.AreEqual("hello", itemEvent.Item);
		    Assert.AreEqual(ItemEventType.ADDED, itemEvent.EventType);
			addLatch.Signal();
		}
		
		public void itemRemoved<String>(ItemEvent<String> itemEvent) {
		    Assert.AreEqual("hello", itemEvent.Item);
			Assert.AreEqual(ItemEventType.REMOVED, itemEvent.EventType);
		    removeLatch.Signal();
		}
	}

    [Test]
    [Ignore]
    public void TenTimesAddRemoveItemListener()  {
        int count = 10;
        CountdownEvent latch = new CountdownEvent(count);
			
				ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				for (int i = 0; i < count; i++) {
	                addRemoveItemListener();
	                latch.Signal();
	             }
			});
        
        Assert.IsTrue(latch.Wait(20000));
    }

    [Test]
    public void destroy() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("destroy");
        for (int i = 0; i < 100; i++) {
            Assert.IsTrue(set.Add(i));
        }
        Hazelcast.Core.ISet<int?> set2 = hClient.getSet<int?>("destroy");
        Assert.IsTrue(set == set2);
        Assert.IsTrue(set.getId().Equals(set2.getId()));
        set.destroy();
        set2 = hClient.getSet<int?>("destroy");
        Assert.IsFalse(set == set2);
//        for(int i=0;i<100;i++){
//        	assertNull(list2.get(i));
//        }
			set.destroy();
    }

    [Test]
    public void add() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("add");
        int count = 100;
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Add(i));
        }
        for (int i = 0; i < count; i++) {
            Assert.IsFalse(set.Add(i));
        }
        Assert.AreEqual(count, set.Count);
			set.destroy();
    }

    [Test]
    public void contains() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("contains");
        int count = 100;
        for (int i = 0; i < count; i++) {
            set.Add(i);
        }
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Contains(i));
        }
        for (int i = count; i < 2 * count; i++) {
            Assert.IsFalse(set.Contains(i));
        }
			set.destroy();
    }

    [Test]
    public void addAll() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("addAll");
        List<int?> arr = new List<int?>();
        int count = 100;
        for (int i = 0; i < count; i++) {
            arr.Add(i);
        }
		foreach( int? i in arr){
			Assert.IsTrue(set.Add(i));		
		}	
			set.destroy();
			
    }

    [Test]
    public void containsAll() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("containsAll");
        List<int?> arrList = new List<int?>();
        int count = 100;
        for (int i = 0; i < count; i++) {
            arrList.Add(i);
        }
        foreach( int? i in arrList){
			Assert.IsTrue(set.Add(i));		
		}
		foreach( int? i in arrList){
			Assert.IsTrue(set.Contains(i));		
		}
        
        arrList[(int) count / 2] =  count + 1;
        bool contains = true;
		foreach( int? i in arrList){
			if(!set.Contains(i)){
					contains = false;
					break;
			}		
		}
		Assert.IsFalse(contains);	
			set.destroy();
    }

    [Test]
    public void size() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("size");
        int count = 100;
        Assert.IsTrue(set.Count==0);
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Add(i));
        }
        Assert.AreEqual(count, set.size());
        for (int i = 0; i < count / 2; i++) {
            Assert.IsFalse(set.Add(i));
        }
        Assert.IsFalse(set.Count==0);
        Assert.AreEqual(count, set.Count);
			set.destroy();
    }

    [Test]
    public void remove() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("remove");
        int count = 100;
        Assert.IsTrue(set.Count==0);
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Add(i));
        }
        Assert.AreEqual(count, set.size());
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Remove(i));
        }
        Assert.IsTrue(set.Count==0);
        for (int i = count; i < 2 * count; i++) {
            Assert.IsFalse(set.Remove( i));
        }
			set.destroy();
    }

    [Test]
    public void clear() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("clear");
        int count = 100;
        Assert.IsTrue(set.Count==0);
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(set.Add(i));
        }
        Assert.AreEqual(count, set.size());
        set.Clear();
        Assert.IsTrue(set.Count==0);
			set.destroy();
    }

	    [Test]
	    public void addIterateAndRemove() {
	        HazelcastClient hClient = getHazelcastClient();
	        Hazelcast.Core.ISet<int?> set = hClient.getSet<int?>("iterate");
	        set.Add(1);
	        set.Add(2);
	        set.Add(2);
	        set.Add(3);
			IEnumerator<int? > e  = set.GetEnumerator();
			while(e.MoveNext()){
				int? i= e.Current;
				set.Remove(i);
			}	
        	Assert.IsTrue(set.Count==0);
    		set.destroy();
		}
		
	}
}

