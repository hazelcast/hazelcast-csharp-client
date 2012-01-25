using System;
using NUnit.Framework;
using System.Threading;
using Hazelcast.Core;
using System.Collections.Generic;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class ListTest: HazelcastTest
	{
	[Test]
	[ExpectedException(typeof(NullReferenceException)) ]
    public void addNull() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<String> list = hClient.getList<String>("addNull");
        list.Add(null);
    }

    [Test]
    public void getListName() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<String> list = hClient.getList<String>("getListName");
        Assert.AreEqual("getListName", list.getName());
			list.destroy();
    }

    [Test]
	[Ignore]
    public void addRemoveItemListener() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<String> list = hClient.getList<String>("addRemoveItemListenerList");
        CountdownEvent addLatch = new CountdownEvent(4);
        CountdownEvent removeLatch = new CountdownEvent(4);
        MyItemListener<String> listener = new MyItemListener<String>(addLatch, removeLatch);
        list.addItemListener(listener, true);
        list.Add("hello");
        list.Add("hello");
        list.Remove("hello");
        list.Remove("hello");
        list.removeItemListener(listener);
        list.Add("hello");
        list.Add("hello");
        list.Remove("hello");
        list.Remove("hello");
        Thread.Sleep(100);
        Assert.AreEqual(2, addLatch.CurrentCount);
        Assert.AreEqual(2, removeLatch.CurrentCount);
			list.destroy();
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
		/*
    [Test]
    public void testListItemListener()  {
        CountdownEvent latch = new CountdownEvent(2);
        String name = "testListListener";
        listener(latch, getHazelcastInstance().<String>getList(name), getHazelcastClient().<String>getList(name));
    }

    [Test]
    public void testListItemListenerOtherWay()  {
        CountdownEvent latch = new CountdownEvent(2);
        String name = "testListListener";
        listener(latch, getHazelcastClient().<String>getList(name), getHazelcastInstance().<String>getList(name));
    }

    private void listener(CountdownEvent latch, IList<String> listOperation, IList<String> listListener) {
        listListener.addItemListener(new ItemListener<String>() {
            public void itemAdded(ItemEvent<String> itemEvent) {
                Assert.AreEqual("hello", itemEvent.getItem());
                latch.countDown();
            }

            public void itemRemoved(ItemEvent<String> itemEvent) {
                Assert.AreEqual("hello", itemEvent.getItem());
                latch.countDown();
            }
        }, true);
        listOperation.add("hello");
        listOperation.remove("hello");
        try {
            Assert.IsTrue(latch.await(5, TimeUnit.SECONDS));
        } catch (InterruptedException ignored) {
        }
    }
		*/
    [Test]
    public void testListAddFromServerGetFromClient() {
        HazelcastClient client = getHazelcastClient();
        String name = "testListAddFromServerGetFromClient";
        client.getList<String>(name).Add("message");
        Assert.IsTrue(client.getList<String>(name).Contains("message"));
		client.getList<String>(name).destroy();
    }

    [Test]
    public void destroy() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("destroy");
        
		for (int i = 0; i < 100; i++) {
            list.Add(i);
        }
        Hazelcast.Core.IList<int?> list2 = hClient.getList<int?>("destroy");
        Assert.IsTrue(list == list2);
        Assert.IsTrue(list.getId().Equals(list2.getId()));
        list.destroy();
        list2 = hClient.getList<int?>("destroy");
        Assert.IsFalse(list == list2);
			list.destroy();
    }

    [Test]
    public void add() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("add");
        int count = 100;
        for (int i = 0; i < count; i++) {
            list.Add(i);
		}
		for (int i = 0; i < count; i++) {
        	Assert.IsTrue(list.Contains(i));
		}
		list.destroy();
			
    }

    [Test]
    public void contains() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("contains");
        int count = 100;
        for (int i = 0; i < count; i++) {
            list.Add(i);
        }
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(list.Contains(i));
        }
        for (int i = count; i < 2 * count; i++) {
            Assert.IsFalse(list.Contains(i));
        }
			list.destroy();
    }
		
   

    [Test]
    public void size() {			
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("size");
        list.Clear();
		int count = 100;
        Assert.IsTrue(list.Count==0);
        for (int i = 0; i < count; i++) {
            list.Add(i);
        }
        Assert.AreEqual(count, list.Count);
        for (int i = 0; i < count / 2; i++) {
            list.Add(i);
        }
        Assert.IsFalse(list.Count==0);
        Assert.AreEqual(count + count / 2, list.Count);
		list.destroy();
    }

    [Test]
    public void remove() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("remove");
        int count = 100;
        Assert.IsTrue(list.Count==0);
        for (int i = 0; i < count; i++) {
            list.Add(i);
        }
        Assert.AreEqual(count, list.Count);
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(list.Remove(i));
        }
        Assert.IsTrue(list.Count==0);
        for (int i = count; i < 2 * count; i++) {
            Assert.IsFalse(list.Remove(i));
        }
			list.destroy();
    }

    [Test]
    public void clear() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("clear");
        int count = 100;
        Assert.IsTrue(list.Count==0);
        for (int i = 0; i < count; i++) {
            list.Add(i);
        }
        Assert.AreEqual(count, list.Count);
        list.Clear();
        Assert.IsTrue(list.Count==0, "List is not empty " + list.Count);
			list.destroy();
    }

	    [Test]
	    public void iterate() {
	        HazelcastClient hClient = getHazelcastClient();
	        Hazelcast.Core.IList<int?> list = hClient.getList<int?>("iterate");
	        list.Add(1);
	        list.Add(2);
	        list.Add(2);
	        list.Add(3);
	        
			Assert.AreEqual(4, list.Count);
	        System.Collections.Generic.IEnumerator<int?> enm = list.GetEnumerator();
			while(enm.MoveNext()) {
				int? i = enm.Current;
				list.Remove(i);
			}
	        Assert.IsTrue(list.Count==0);
			list.destroy();
	    }
	}
}