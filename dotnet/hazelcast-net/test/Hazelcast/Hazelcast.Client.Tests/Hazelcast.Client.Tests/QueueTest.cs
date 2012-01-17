using System;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Client.Tests
{
	//[TestFixture()]
	public class QueueTest: HazelcastTest
	{
		
    [Test]
	[ExpectedException(typeof(NullReferenceException)) ]
    public void testPutNull(){
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testPutNull");
        queue.put(null);
    }
		
		
    [Test]
    public void testQueueName() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testQueueName");
        Assert.AreEqual("testQueueName", queue.getName());
    }
		
		

    [Test]
    public void testQueueOffer(){
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testQueueOffer");
        
		Assert.IsTrue(queue.offer("a"));
        Assert.IsTrue(queue.offer("b", 10));
        Assert.AreEqual("a", queue.poll());
        Assert.AreEqual("b", queue.poll());
    }
	
    [Test]
    public void testQueuePoll() {
        HazelcastClient hClient = getHazelcastClient();
        
		CountdownEvent latch = new CountdownEvent(1);
        IQueue<String> queue = hClient.getQueue<String>("testQueuePoll");
        Assert.IsTrue(queue.offer("a"));
        Assert.AreEqual("a", queue.poll());
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				Thread.Sleep(100);
                Assert.AreEqual("b", queue.poll(100));
                latch.Signal();
			});
			
        Thread.Sleep(50);
        Assert.IsTrue(queue.offer("b"));
        Assert.IsTrue(latch.Wait(200 ));
	}

    [Test]
    public void testQueuePeek() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testQueuePeek");
        Assert.IsTrue(queue.offer("a"));
        Assert.AreEqual("a", queue.peek());
    }
		
		
		
    [Test]
    public void testQueueRemove() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testQueueRemove");
        Assert.IsTrue(queue.offer("a"));
        Assert.AreEqual("a", queue.Remove());
    }

    [Test]
    public void element() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("element");
        Assert.IsTrue(queue.offer("a"));
        Assert.AreEqual("a", queue.element());
    }
		
	
    [Test]
    public void addAll() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("addAll");
        List<String> list = new List<String>();
        list.Add("a");
        list.Add("b");
        Assert.IsTrue(queue.addAll(list));
        Assert.AreEqual("a", queue.poll());
        Assert.AreEqual("b", queue.poll());
    }

    [Test]
    public void clear() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("clear");
        List<String> list = new List<String>();
        list.Add("a");
        list.Add("b");
        Assert.IsTrue(queue.Count == 0);
        Assert.IsTrue(queue.addAll(list));
        Assert.IsTrue(queue.Count == 2);
        queue.Clear();
        Assert.IsTrue(queue.Count == 0);
    }

    [Test]
    public void containsAll() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("containsAll");
        List<String> list = new List<String>();
        list.Add("a");
        list.Add("b");
        Assert.IsTrue(queue.Count == 0);
        Assert.IsTrue(queue.addAll(list));
        Assert.IsTrue(queue.Count == 2);
		foreach( String s in list){
				Assert.IsTrue(queue.Contains(s));		
		}
		queue.Clear();	
    }
		
		

    [Test]
    public void isEmpty() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("isEmpty");
        Assert.IsTrue(queue.Count == 0);
        queue.offer("asd");
        Assert.IsFalse(queue.Count == 0);
		queue.Clear();
    }

    [Test]
    public void iterator() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("iterator");
        Assert.IsTrue(queue.Count == 0);
        int count = 100;
        Dictionary<int, int> map = new Dictionary<int, int>();
        for (int i = 0; i < count; i++) {
            queue.offer("" + i);
            map.Add(i, 1);
        }
        IEnumerator<String> it = queue.GetEnumerator();
        while (it.MoveNext()) {
            String o = it.Current;
            map[Int32.Parse(o)] = map[Int32.Parse(o)] - 1;
        }
        for (int i = 0; i < count; i++) {
            Assert.IsTrue(map[i] == 0);
        }
			queue.Clear();
    }

    [Test]
    public void removeAll() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("removeAll");
        Assert.IsTrue(queue.Count==0);
        int count = 100;
        Dictionary<int, int> map = new Dictionary<int, int>();
        for (int i = 0; i < count; i++) {
            queue.offer("" + i);
            map.Add(i, 1);
        }
        List<String> list = new List<String>();
        for (int i = 0; i < count / 2; i++) {
            list.Add(""+i);
        }
			foreach(String s in list){
				queue.Remove(s);		
			}
        Assert.IsTrue(queue.Count == count / 2);
			queue.Clear();
    }
		
		
    [Test]
    public void testIterator() {
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testIterator");
        Assert.IsTrue(queue.Count==0);
        int count = 100;
        Dictionary<int, int> map = new Dictionary<int, int>();
        for (int i = 0; i < count; i++) {
            queue.offer("" + i);
            map.Add(i, 1);
        }
        IEnumerator<String> it = queue.GetEnumerator();
        while (it.MoveNext()) {
            String item = it.Current;
            map.Remove(Int32.Parse(item));
            queue.Remove(item);
        }
        Assert.AreEqual(0, queue.Count);
        Assert.AreEqual(0, map.Count);
    }
		
		
	class MyItemListener<String> :ItemListener<String>{
		CountdownEvent latch;
				
		public MyItemListener(CountdownEvent latch){
			this.latch = latch;		
		}	
		
		public void itemAdded<String>(ItemEvent<String> itemEvent) {
		    Assert.AreEqual("hello", itemEvent.Item);
		    latch.Signal();
		}
		
		public void itemRemoved<String>(ItemEvent<String> itemEvent) {
		    Assert.AreEqual("hello", itemEvent.Item);
		    latch.Signal();
		}
	}
		
    [Test]
    public void testQItemListener() {
		CountdownEvent latch = new CountdownEvent(2);
        String name = "testListListener";
        IQueue<String> qOperation = getHazelcastClient().getQueue<String>(name);
        IQueue<String> qListener = getHazelcastClient().getQueue<String>(name);
        
			
			
        qListener.addItemListener(new MyItemListener<String>(latch), true);
        qOperation.offer("hello");
        qOperation.poll();
        Assert.IsTrue(latch.Wait(5000));
        
    }
	
    [Test]
    public void testQueueItemListener() {
        CountdownEvent latch = new CountdownEvent(2);
        HazelcastClient hClient = getHazelcastClient();
        IQueue<String> queue = hClient.getQueue<String>("testQueueListener");
        queue.addItemListener(new MyItemListener<String>(latch), true);
        queue.offer("hello");
        Assert.AreEqual("hello", queue.poll());
     	Assert.IsTrue(latch.Wait(5000));
        
    }
	
	}
}

