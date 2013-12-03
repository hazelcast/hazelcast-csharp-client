using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Hazelcast.Client;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Query;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class MultiMapTest :HazelcastTest
	{
		
		public MultiMapTest ()
		{
		}
		
		[Test]
		[ExpectedException(typeof(NullReferenceException)) ]
	    public void testPutNull() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<int?, String> map = hClient.getMultiMap<int?, String>("testPutNull");
	        map.put(1, null);
			map.destroy();
	    }
	
	    [Test]
	    public void putToMultiMap() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("putToMultiMap");
	        Assert.IsTrue(multiMap.put("a", 1));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void removeFromMultiMap() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("removeFromMultiMap");
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.remove("a", 1));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void containsKey() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("containsKey");
	        Assert.IsFalse(multiMap.containsKey("a"));
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.containsKey("a"));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void containsValue() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("containsValue");
	        Assert.IsFalse(multiMap.containsValue(1));
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.containsValue(1));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void containsEntry() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("containsEntry");
	        Assert.IsFalse(multiMap.containsEntry("a", 1));
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.containsEntry("a", 1));
	        Assert.IsFalse(multiMap.containsEntry("a", 2));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void size() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("size");
	        Assert.AreEqual(0, multiMap.size());
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.AreEqual(1, multiMap.size());
	        Assert.IsTrue(multiMap.put("a", 2));
	        Assert.AreEqual(2, multiMap.size());
			multiMap.destroy();
	    }
	
	    [Test]
	    public void get()  {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("get");
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.put("a", 2));
	        Dictionary<int?, CountdownEvent> map = new Dictionary<int?, CountdownEvent>();
	        map.Add(1, new CountdownEvent(1));
	        map.Add(2, new CountdownEvent(1));
	        System.Collections.Generic.ICollection<int?> collection = multiMap.get("a");
	        //Assert.AreEqual(Values.class, collection.getClass());
	        Assert.AreEqual(2, collection.Count);
	        for (IEnumerator<int?> it = collection.GetEnumerator(); it.MoveNext();) {
	            int? o = it.Current;
	            map[o].Signal();
	        }
	        Assert.IsTrue(map[1].Wait(10000));
	        Assert.IsTrue(map[2].Wait(10000));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void removeKey()  {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, int?> multiMap = hClient.getMultiMap<String, int?>("removeKey");
	        Assert.IsTrue(multiMap.put("a", 1));
	        Assert.IsTrue(multiMap.put("a", 2));
	        Dictionary<int?, CountdownEvent> map = new Dictionary<int?, CountdownEvent>();
	        map.Add(1, new CountdownEvent(1));
	        map.Add(2, new CountdownEvent(1));
	        System.Collections.Generic.ICollection<int?> collection = multiMap.remove("a");
	        //Assert.AreEqual(Values.class, collection.getClass());
	        Assert.AreEqual(2, collection.Count);
	        for (IEnumerator<int?> it = collection.GetEnumerator(); it.MoveNext();) {
	            int? o = it.Current;
	            map[o].Signal();
	        }
	        Assert.IsTrue(map[1].Wait(10000));
	        Assert.IsTrue(map[2].Wait(10000));
			multiMap.destroy();
	    }
	
	    [Test]
	    public void keySet()  {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> multiMap = hClient.getMultiMap<String, String>("keySet");
	        int count = 100;
	        for (int i = 0; i < count; i++) {
	            for (int j = 0; j <= i; j++) {
	                multiMap.put(""+i, ""+j);
	            }
	        }
	        Assert.AreEqual(count * (count + 1) / 2, multiMap.size());
	        System.Collections.Generic.ICollection<String> set = multiMap.keySet();
	        Assert.AreEqual(count, set.Count);
	        for (int i = 0; i < count; i++) {
	            set.Remove(""+i);
	        }
	        Assert.AreEqual(0, set.Count);
	    	multiMap.destroy();
		}
	
	    [Test]
	    public void testMultiMapPutAndGet() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapPutAndGet");
	        map.put("Hello", "World");
	       System.Collections.Generic.ICollection<String> values = map.get("Hello");
			IEnumerator<String> it = values.GetEnumerator();
			it.MoveNext();
	        Assert.AreEqual("World", it.Current);
	        map.put("Hello", "Europe");
	        map.put("Hello", "America");
	        map.put("Hello", "Asia");
	        map.put("Hello", "Africa");
	        map.put("Hello", "Antarctica");
	        map.put("Hello", "Australia");
	        values = map.get("Hello");
	        Assert.AreEqual(7, values.Count);
	        Assert.IsTrue(map.containsKey("Hello"));
	        Assert.IsFalse(map.containsKey("Hi"));
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapGetNameAndType() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapGetNameAndType");
	        Assert.AreEqual("testMultiMapGetNameAndType", map.getName());
	        Assert.IsTrue(map.getInstanceType().Equals(InstanceType.MULTIMAP));
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapClear() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapClear");
	        map.put("Hello", "World");
	        Assert.AreEqual(1, map.size());
	        map.clear();
	        Assert.AreEqual(0, map.size());
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapContainsKey() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapContainsKey");
	        map.put("Hello", "World");
	        Assert.IsTrue(map.containsKey("Hello"));
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapContainsValue() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapContainsValue");
	        map.put("Hello", "World");
	        Assert.IsTrue(map.containsValue("World"));
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapContainsEntry() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapContainsEntry");
	        map.put("Hello", "World");
	        Assert.IsTrue(map.containsEntry("Hello", "World"));
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapKeySet() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapKeySet");
	        map.put("Hello", "World");
	        map.put("Hello", "Europe");
	        map.put("Hello", "America");
	        map.put("Hello", "Asia");
	        map.put("Hello", "Africa");
	        map.put("Hello", "Antarctica");
	        map.put("Hello", "Australia");
	        System.Collections.Generic.ICollection<String> keys = map.keySet();
	        Assert.AreEqual(1, keys.Count);
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapRemove() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapRemove");
	        map.put("Hello", "World");
	        map.put("Hello", "Europe");
	        map.put("Hello", "America");
	        map.put("Hello", "Asia");
	        map.put("Hello", "Africa");
	        map.put("Hello", "Antarctica");
	        map.put("Hello", "Australia");
	        Assert.AreEqual(7, map.size());
	        Assert.AreEqual(1, map.keySet().Count);
	        System.Collections.Generic.ICollection<String> values = map.remove("Hello");
	        Assert.AreEqual(7, values.Count);
	        Assert.AreEqual(0, map.size());
	        Assert.AreEqual(0, map.keySet().Count);
	        map.put("Hello", "World");
	        Assert.AreEqual(1, map.size());
	        Assert.AreEqual(1, map.keySet().Count);
			map.destroy();
	    }
	
	    [Test]
	    public void testMultiMapRemoveEntries() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<String, String> map = hClient.getMultiMap<String, String>("testMultiMapRemoveEntries");
	        map.put("Hello", "World");
	        map.put("Hello", "Europe");
	        map.put("Hello", "America");
	        map.put("Hello", "Asia");
	        map.put("Hello", "Africa");
	        map.put("Hello", "Antartica");
	        map.put("Hello", "Australia");
	        bool removed = map.remove("Hello", "World");
	        Assert.IsTrue(removed);
	        Assert.AreEqual(6, map.size());
			map.destroy();
	    }
	
	   
	    [Test]
	    public void testMultiMapValueCount() {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<int?, String> map = hClient.getMultiMap<int?, String>("testMultiMapValueCount");
	        map.put(1, "World");
	        map.put(2, "Africa");
	        map.put(1, "America");
	        map.put(2, "Antarctica");
	        map.put(1, "Asia");
	        map.put(1, "Europe");
	        map.put(2, "Australia");
	        Assert.AreEqual(4, map.valueCount(1));
	        Assert.AreEqual(3, map.valueCount(2));
			map.destroy();
	    }
	
	    /*[Test]
	    [Ignore]
	    public void testLotsOfRemove()  {
	        HazelcastClient hClient = getHazelcastClient();
	       IMultiMap<int?, String> map = hClient.getMultiMap<int?, String(>"testLotsOfRemove");
	        map.put(1, "adam");
	        Atomicboolrunning = new AtomicBoolean(true);
	        Atomicint? p = new Atomicint?(0);
	        Atomicint? r = new Atomicint?(0);
	        Thread.sleep(1000);
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				while (running.get()) {
	                    map.put(1, "" + Math.random());
	                    p.incrementAndGet();
	                }
			});
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				while (running.get()) {
	                    map.remove(1);
	                    r.incrementAndGet();
	                }
			});
			
			
	        CountdownEvent latch = new CountdownEvent(1);
	        
		ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				int ip = p.get();
	                int ir = r.get();
	                try {
	                    Thread.sleep(1000);
	                } catch (InterruptedException e) {
	                }
	                if (p.get() == ip || r.get() == ir) {
	                    Console.WriteLine("STUCK p= " + p.get() + "::: r" + r.get());
	                } else {
	                    latch.countDown();
	                }
			});
			

	        Assert.IsTrue(latch.Wait(5000));
	        running.set(false);
	    }*/
	}
}

