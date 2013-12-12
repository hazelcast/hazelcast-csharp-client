using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{

	[TestFixture]
	public class ClientMapTest:HazelcastBaseTest
	{
		internal const string name = "test";

		internal static IHazelcastMap<object,object> map;

		//
		[SetUp]
		public static void Init()
		{
		    InitClient();
            map = client.GetMap<object, object>(name);
		}

		[TearDown]
		public static void Destroy()
		{
			map.Clear();
            //client.GetLifecycleService().Shutdown();
		}


        ///// <exception cref="System.Exception"></exception>
        //[Test]
        //public virtual void TestIssue537()
        //{
        //    CountdownEvent latch = new CountdownEvent(2);
        //    CountdownEvent nullLatch = new CountdownEvent(2);
        //    EntryListener listener = new _EntryListener_81(latch, nullLatch);
        //    string id = map.AddEntryListener(listener, true);
        //    map.Put("key1", new GenericEvent("value1"), 2, TimeUnit.Seconds);
        //    Assert.IsTrue(latch.Await(10, TimeUnit.Seconds));
        //    Assert.IsTrue(nullLatch.Await(1, TimeUnit.Seconds));
        //    map.RemoveEntryListener(id);
        //    map.Put("key2", new GenericEvent("value2"));
        //    Assert.AreEqual(1, map.Size());
        //}

        //private sealed class _EntryListener_81 : EntryListener
        //{
        //    public _EntryListener_81(CountdownEvent latch, CountdownEvent nullLatch)
        //    {
        //        this.latch = latch;
        //        this.nullLatch = nullLatch;
        //    }

        //    public void EntryAdded(EntryEvent @event)
        //    {
        //        latch.Signal();
        //    }

        //    public void EntryRemoved(EntryEvent @event)
        //    {
        //    }

        //    public void EntryUpdated(EntryEvent @event)
        //    {
        //    }

        //    public void EntryEvicted(EntryEvent @event)
        //    {
        //        object value = @event.GetValue();
        //        object oldValue = @event.GetOldValue();
        //        if (value != null)
        //        {
        //            nullLatch.Signal();
        //        }
        //        if (oldValue != null)
        //        {
        //            nullLatch.Signal();
        //        }
        //        latch.Signal();
        //    }

        //    private readonly CountdownEvent latch;

        //    private readonly CountdownEvent nullLatch;
        //}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestContains()
		{
			FillMap();
			Assert.IsFalse(map.ContainsKey("key10"));
			Assert.IsTrue(map.ContainsKey("key1"));
			Assert.IsFalse(map.ContainsValue("value10"));
			Assert.IsTrue(map.ContainsValue("value1"));
		}

		[Test]
		public virtual void TestGet()
		{
			FillMap();
			for (int i = 0; i < 10; i++)
			{
				object o = map.Get("key" + i);
				Assert.AreEqual("value" + i, o);
			}
		}

		[Test]
		public virtual void TestRemoveAndDelete()
		{
			FillMap();
			Assert.IsNull(map.Remove("key10"));
			map.Delete("key9");
			Assert.AreEqual(9, map.Size());
			for (int i = 0; i < 9; i++)
			{
				object o = map.Remove("key" + i);
				Assert.AreEqual("value" + i, o);
			}
			Assert.AreEqual(0, map.Size());
		}

		[Test]
		public virtual void TestRemoveIfSame()
		{
			FillMap();
			Assert.IsFalse(map.Remove("key2", "value"));
			Assert.AreEqual(10, map.Size());
			Assert.IsTrue(map.Remove("key2", "value2"));
			Assert.AreEqual(9, map.Size());
		}

		[Test]
		public virtual void Flush()
		{
		}

		//TODO map store
		[Test]
		public virtual void TestGetAllPutAll()
		{
			IDictionary<object,object> mm = new Dictionary<object, object>();
			for (int i = 0; i < 100; i++)
			{
				mm.Add(i, i);
			}
			map.PutAll(mm);
			Assert.AreEqual(map.Size(), 100);
			for (int i_1 = 0; i_1 < 100; i_1++)
			{
				Assert.AreEqual(map.Get(i_1), i_1);
			}
			var ss = new HashSet<object> {1, 3};

		    var m2 = map.GetAll(ss);
			Assert.AreEqual(m2.Count, 2);

		    object gv;
		    m2.TryGetValue(1, out gv);
			Assert.AreEqual(gv, 1);

            m2.TryGetValue(3, out gv);
			Assert.AreEqual(gv, 3);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAsyncGet()
		{
			FillMap();
			var f = map.GetAsync("key1");
		
            Assert.False(f.IsCompleted);

			object o = f.Result;
			Assert.AreEqual("value1", o);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAsyncPut()
		{
			FillMap();
			var f = map.PutAsync("key3", "value");

            Assert.False(f.IsCompleted);

            object o = f.Result;
			Assert.AreEqual("value3", o);
			Assert.AreEqual("value", map.Get("key3"));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAsyncPutWithTtl()
		{
            var latch = new CountdownEvent(1);

		    map.AddEntryListener(new EntryAdapter<object, object>(
                delegate { },
		        delegate { },
		        delegate { },
                delegate { latch.Signal(); }
		        ), true);

			var f1 = map.PutAsync("key", "value1", 3, TimeUnit.SECONDS);
            Assert.IsNull(f1.Result);
			Assert.AreEqual("value1", map.Get("key"));

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.IsNull(map.Get("key"));
		}

	 


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAsyncRemove()
		{
			FillMap();
			var f = map.RemoveAsync("key4");
            Assert.False(f.IsCompleted);

            object o = f.Result;
			Assert.AreEqual("value4", o);
			Assert.AreEqual(9, map.Size());
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTryPutRemove()
		{
			Assert.IsTrue(map.TryPut("key1", "value1", 1, TimeUnit.SECONDS));
			Assert.IsTrue(map.TryPut("key2", "value2", 1, TimeUnit.SECONDS));
			map.Lock("key1");
			map.Lock("key2");
			CountdownEvent latch = new CountdownEvent(2);

            var t1 = new Thread(delegate(object o)
            {
                bool result = map.TryPut("key1", "value3", 1, TimeUnit.SECONDS);
                if (!result)
                {
                    latch.Signal();
                }
            });

            var t2 = new Thread(delegate(object o)
            {
                bool result = ClientMapTest.map.TryRemove("key2", 1, TimeUnit.SECONDS);
                if (!result)
                {
                    latch.Signal();
                }
            });

            t1.Start();
			t2.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
			Assert.AreEqual("value1", map.Get("key1"));
			Assert.AreEqual("value2", map.Get("key2"));
			
            map.ForceUnlock("key1");
			map.ForceUnlock("key2");
		}


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutTtl()
		{
			map.Put("key1", "value1", 1, TimeUnit.SECONDS);
			Assert.IsNotNull(map.Get("key1"));
			Thread.Sleep(2000);
			Assert.IsNull(map.Get("key1"));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutIfAbsent()
		{
			Assert.IsNull(map.PutIfAbsent("key1", "value1"));
			Assert.AreEqual("value1", map.PutIfAbsent("key1", "value3"));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutIfAbsentTtl()
		{
			Assert.IsNull(map.PutIfAbsent("key1", "value1", 1, TimeUnit.SECONDS));
			Assert.AreEqual("value1", map.PutIfAbsent("key1", "value3", 1, TimeUnit.SECONDS));
			Thread.Sleep(2000);
			Assert.IsNull(map.PutIfAbsent("key1", "value3", 1, TimeUnit.SECONDS));
			Assert.AreEqual("value3", map.PutIfAbsent("key1", "value4", 1, TimeUnit.SECONDS));
			Thread.Sleep(2000);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestSet()
		{
			map.Set("key1", "value1");
			Assert.AreEqual("value1", map.Get("key1"));
			map.Set("key1", "value2");
			Assert.AreEqual("value2", map.Get("key1"));
			map.Set("key1", "value3", 1, TimeUnit.SECONDS);
			Assert.AreEqual("value3", map.Get("key1"));
			Thread.Sleep(2000);
			Assert.IsNull(map.Get("key1"));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestPutTransient()
		{
		}

		//TODO mapstore
		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLock()
		{
			map.Put("key1", "value1");
			Assert.AreEqual("value1", map.Get("key1"));
			map.Lock("key1");
            CountdownEvent latch = new CountdownEvent(1);

            var t1 = new Thread(delegate(object o)
            {
                map.TryPut("key1", "value2", 1, TimeUnit.SECONDS);
                latch.Signal();
            });
            t1.Start();
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
			Assert.AreEqual("value1", map.Get("key1"));
			map.ForceUnlock("key1");
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLockTtl()
		{
			map.Put("key1", "value1");
			Assert.AreEqual("value1", map.Get("key1"));
			map.Lock("key1", 2, TimeUnit.SECONDS);
			CountdownEvent latch = new CountdownEvent(1);
            var t1 = new Thread(delegate(object o)
            {
                map.TryPut("key1", "value2", 5, TimeUnit.SECONDS);
                latch.Signal();
            });

			t1.Start();
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.IsFalse(map.IsLocked("key1"));
			Assert.AreEqual("value2", map.Get("key1"));
			map.ForceUnlock("key1");
		}

		

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLockTtl2()
		{
			map.Lock("key1", 3, TimeUnit.SECONDS);
			CountdownEvent latch = new CountdownEvent(2);
            var t1 = new Thread(delegate(object o)
            {
                if (!map.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (map.TryLock("key1", 5, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                    
                }
            });

			t1.Start();
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			map.ForceUnlock("key1");
		}

        ///// <exception cref="System.Exception"></exception>
        //[Test]
        //public virtual void TestTryLock()
        //{
        //    //        final IMap tempMap = server.getMap(name);
        //    IHazelcastMap<> tempMap = map;
        //    Assert.IsTrue(tempMap.TryLock("key1", 2, TimeUnit.SECONDS));
        //    CountdownEvent latch = new CountdownEvent(1);

        //    var t1 = new Thread(delegate(object o)
        //    {
        //       try
        //        {
        //            if (!tempMap.TryLock("key1", 2, TimeUnit.SECONDS))
        //            {
        //                latch.Signal();
        //            }
        //        }
        //        catch (Exception e)
        //        {
					
        //        } 

        //    });

        //    t1.Start();

        //    Assert.IsTrue(latch.Await(100, TimeUnit.Seconds));
        //    Assert.IsTrue(tempMap.IsLocked("key1"));
        //    CountdownEvent latch2 = new CountdownEvent(1);

        //    new _Thread_414(tempMap, latch2).Start();
			
        //    Thread.Sleep(1000);
			
        //    tempMap.Unlock("key1");
        //    Assert.IsTrue(latch2.Await(100, TimeUnit.Seconds));
        //    Assert.IsTrue(tempMap.IsLocked("key1"));
        //    tempMap.ForceUnlock("key1");
        //}

        //private sealed class _Thread_398 : Hazelcast.Net.Ext.Thread
        //{
        //    public _Thread_398(IMap tempMap, CountdownEvent latch)
        //    {
        //        this.tempMap = tempMap;
        //        this.latch = latch;
        //    }

        //    public override void Run()
        //    {
        //        try
        //        {
        //            if (!tempMap.TryLock("key1", 2, TimeUnit.Seconds))
        //            {
        //                latch.Signal();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Sharpen.Runtime.PrintStackTrace(e);
        //        }
        //    }

        //    private readonly IMap tempMap;

        //    private readonly CountdownEvent latch;
        //}

        //private sealed class _Thread_414 : Hazelcast.Net.Ext.Thread
        //{
        //    public _Thread_414(IMap tempMap, CountdownEvent latch2)
        //    {
        //        this.tempMap = tempMap;
        //        this.latch2 = latch2;
        //    }

        //    public override void Run()
        //    {
        //        try
        //        {
        //            if (tempMap.TryLock("key1", 20, TimeUnit.Seconds))
        //            {
        //                latch2.Signal();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Sharpen.Runtime.PrintStackTrace(e);
        //        }
        //    }

        //    private readonly IMap tempMap;

        //    private readonly CountdownEvent latch2;
        //}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestForceUnlock()
		{
			map.Lock("key1");
            CountdownEvent latch = new CountdownEvent(1);

            var t1 = new Thread(delegate(object o)
            {
                map.ForceUnlock("key1");
                latch.Signal();
            });

			t1.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsFalse(map.IsLocked("key1"));
		}


        [Test]
		public void TestValues() 
        {
		    FillMap();
		
		    var values = map.Values(new SqlPredicate("this == value1"));
		    Assert.AreEqual(1, values.Count);
            IEnumerator<object> enumerator = values.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("value1", enumerator.Current);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestReplace()
		{
			Assert.IsNull(map.Replace("key1", "value1"));
			map.Put("key1", "value1");
			Assert.AreEqual("value1", map.Replace("key1", "value2"));
			Assert.AreEqual("value2", map.Get("key1"));
			Assert.IsFalse(map.Replace("key1", "value1", "value3"));
			Assert.AreEqual("value2", map.Get("key1"));
			Assert.IsTrue(map.Replace("key1", "value2", "value3"));
			Assert.AreEqual("value3", map.Get("key1"));
		}

		//    @Test
		//    public void testSubmitToKey() throws Exception {
		//        map.put(1,1);
		//        Future f = map.submitToKey(1, new IncrementorEntryProcessor());
		//        Assert.assertEquals(2,f.get());
		//        Assert.assertEquals(2,map.get(1));
		//    }
		//    @Test
		//    public void testSubmitToNonExistentKey() throws Exception {
		//        Future f = map.submitToKey(11, new IncrementorEntryProcessor());
		//        Assert.assertEquals(1,f.get());
		//        Assert.assertEquals(1,map.get(11));
		//    }
		//    @Test
		//    public void testSubmitToKeyWithCallback() throws  Exception
		//    {
		//        map.put(1,1);
		//        final CountdownEvent latch = new CountdownEvent(1);
		//        ExecutionCallback executionCallback = new ExecutionCallback() {
		//            @Override
		//            public void onResponse(Object response) {
		//                latch.Signal();
		//            }
		//
		//            @Override
		//            public void onFailure(Throwable t) {
		//            }
		//        };
		//
		//        map.submitToKey(1,new IncrementorEntryProcessor(),executionCallback);
		//        Assert.assertTrue(latch.await(5, TimeUnit.SECONDS));
		//        Assert.assertEquals(2,map.get(1));
		//    }
		   
        /// <exception cref="System.Exception"></exception>
		[Test]
        public void testListener()
        {
			CountdownEvent latch1Add = new CountdownEvent(5);
			CountdownEvent latch1Remove = new CountdownEvent(2);
			CountdownEvent latch2Add = new CountdownEvent(1);
			CountdownEvent latch2Remove = new CountdownEvent(1);
            EntryAdapter<object,object> listener1=new EntryAdapter<object, object>(
                delegate(EntryEvent<object, object> @event) { latch1Add.Signal(); },
                delegate(EntryEvent<object, object> @event) { latch1Remove.Signal(); }, 
                delegate(EntryEvent<object, object> @event) {  },
                delegate(EntryEvent<object, object> @event) {  }  ); 

            EntryAdapter<object,object> listener2=new EntryAdapter<object, object>(
                delegate(EntryEvent<object, object> @event)
                {
                    latch2Add.Signal();
                },
                delegate(EntryEvent<object, object> @event)
                {
                    latch2Remove.Signal();
                }, 
                delegate(EntryEvent<object, object> @event) {  },
                delegate(EntryEvent<object, object> @event) {  }  ); 
            
			map.AddEntryListener(listener1, false);
			map.AddEntryListener(listener2, "key3", true);

            Thread.Sleep(1000);

			map.Put("key1", "value1");
			map.Put("key2", "value2");
			map.Put("key3", "value3");
			map.Put("key4", "value4");
			map.Put("key5", "value5");

            Thread.Sleep(1000);

			map.Remove("key1");
			map.Remove("key3");

			Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(1000)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(5)));
        }

        ///// <exception cref="System.Exception"></exception>
        //[Test]
        //public void testPredicateListenerWithPortableKey() throws InterruptedException {
        //    //var tradeMap = client.getMap("tradeMap");
        //    //final CountdownEvent CountdownEvent = new CountdownEvent(1);
        //    //final AtomicInteger atomicInteger = new AtomicInteger(0);

        //    var countdownEvent = new CountdownEvent(1);
        //    EntryAdapter<object,object> listener=new EntryAdapter<object, object>(
        //        delegate(EntryEvent<object, object> @event) { countdownEvent.Signal(); },
        //        delegate(EntryEvent<object, object> @event) {  }, 
        //        delegate(EntryEvent<object, object> @event) {  },
        //        delegate(EntryEvent<object, object> @event) {  }  ); 

        //    //EntryListener listener = new EntryListener() {
        //    //    @Override
        //    //    public void entryAdded(EntryEvent event) {
        //    //        atomicInteger.incrementAndGet();
        //    //        countdownEvent.Signal();
        //    //    }
		
        //    //    @Override
        //    //    public void entryRemoved(EntryEvent event) {
        //    //    }
		
        //    //    @Override
        //    //    public void entryUpdated(EntryEvent event) {
        //    //    }
		
        //    //    @Override
        //    //    public void entryEvicted(EntryEvent event) {
        //    //    }
        //    //};
        //    var key = new AuthenticationRequest(new UsernamePasswordCredentials("a", "b"));
        //    tradeMap.addEntryListener(listener, key, true);

        //    var key2 = new AuthenticationRequest(new UsernamePasswordCredentials("a", "c"));
        //    tradeMap.put(key2, 1);
		    
        //    Assert.assertFalse(CountdownEvent.await(15, TimeUnit.SECONDS));
        //    Assert.assertEquals(0,atomicInteger.get());
        //}

		//    @Test
		//    public void testBasicPredicate() {
		//        fillMap();
		//        final Collection collection = map.values(new SqlPredicate("this == value1"));
		//        Assert.assertEquals("value1", collection.iterator().next());
		//        final Set set = map.keySet(new SqlPredicate("this == value1"));
		//        Assert.assertEquals("key1", set.iterator().next());
		//        final Set<Map.Entry<String, String>> set1 = map.entrySet(new SqlPredicate("this == value1"));
		//        Assert.assertEquals("key1", set1.iterator().next().getKey());
		//        Assert.assertEquals("value1", set1.iterator().next().getValue());
		//    }
		private void FillMap()
		{
			for (int i = 0; i < 10; i++)
			{
				map.Put("key" + i, "value" + i);
			}
		}

        ///// <summary>Issue #923</summary>
        //[Test]
        //public virtual void TestPartitionAwareKey()
        //{
        //    string name = "testPartitionAwareKey";
        //    PartitionAwareKey key = new PartitionAwareKey("key", "123");
        //    string value = "value";
        //    IMap<object, object> map1 = server.GetMap(name);
        //    map1.Put(key, value);
        //    Assert.AreEqual(value, map1.Get(key));
        //    IMap<object, object> map2 = client.GetMap(name);
        //    Assert.AreEqual(value, map2.Get(key));
        //}

        //[System.Serializable]
        //private class PartitionAwareKey : IPartitionAware
        //{
        //    private readonly string key;

        //    private readonly string pk;

        //    public PartitionAwareKey(string key, string pk)
        //    {
        //        this.key = key;
        //        this.pk = pk;
        //    }

        //    public virtual object GetPartitionKey()
        //    {
        //        return pk;
        //    }
        //}

        ///// <summary>Issue #996</summary>
        ///// <exception cref="System.Exception"></exception>
        //[Test]
        //public virtual void TestEntryListener()
        //{
        //    CountdownEvent gateAdd = new CountdownEvent(2);
        //    CountdownEvent gateRemove = new CountdownEvent(1);
        //    CountdownEvent gateEvict = new CountdownEvent(1);
        //    CountdownEvent gateUpdate = new CountdownEvent(1);
        //    string mapName = "testEntryListener";
        //    IMap<object, object> serverMap = server.GetMap(mapName);
        //    serverMap.Put(3, new ClientMapTest.Deal(3));
        //    IMap<object, object> clientMap = client.GetMap(mapName);
        //    Assert.AreEqual(1, clientMap.Size());
        //    EntryListener listener = new ClientMapTest.EntListener(gateAdd, gateRemove, gateEvict, gateUpdate);
        //    //        clientMap.addEntryListener(listener, new SqlPredicate("id=1"), 2, true);
        //    clientMap.Put(2, new ClientMapTest.Deal(1));
        //    clientMap.Put(2, new ClientMapTest.Deal(1));
        //    Sharpen.Collections.Remove(clientMap, 2);
        //    clientMap.Put(2, new ClientMapTest.Deal(1));
        //    clientMap.Evict(2);
        //    Assert.IsTrue(gateAdd.Await(10, TimeUnit.Seconds));
        //    Assert.IsTrue(gateRemove.Await(10, TimeUnit.Seconds));
        //    Assert.IsTrue(gateEvict.Await(10, TimeUnit.Seconds));
        //    Assert.IsTrue(gateUpdate.Await(10, TimeUnit.Seconds));
        //}

        //[System.Serializable]
        //internal class EntListener : EntryListener<int, ClientMapTest.Deal>
        //{
        //    private readonly CountdownEvent _gateAdd;

        //    private readonly CountdownEvent _gateRemove;

        //    private readonly CountdownEvent _gateEvict;

        //    private readonly CountdownEvent _gateUpdate;

        //    internal EntListener(CountdownEvent gateAdd, CountdownEvent gateRemove, CountdownEvent gateEvict, CountdownEvent gateUpdate)
        //    {
        //        _gateAdd = gateAdd;
        //        _gateRemove = gateRemove;
        //        _gateEvict = gateEvict;
        //        _gateUpdate = gateUpdate;
        //    }

        //    public virtual void EntryAdded(EntryEvent<int, ClientMapTest.Deal> arg0)
        //    {
        //        _gateAdd.Signal();
        //    }

        //    public virtual void EntryEvicted(EntryEvent<int, ClientMapTest.Deal> arg0)
        //    {
        //        _gateEvict.Signal();
        //    }

        //    public virtual void EntryRemoved(EntryEvent<int, ClientMapTest.Deal> arg0)
        //    {
        //        _gateRemove.Signal();
        //    }

        //    public virtual void EntryUpdated(EntryEvent<int, ClientMapTest.Deal> arg0)
        //    {
        //        _gateUpdate.Signal();
        //    }
        //}

		[System.Serializable]
		internal class Deal
		{
			internal int id;

			internal Deal(int id)
			{
				this.id = id;
			}

			public virtual int GetId()
			{
				return id;
			}

			public virtual void SetId(int id)
			{
				this.id = id;
			}
		}

        //[System.Serializable]
        //private class IncrementorEntryProcessor : IDataSerializable
        //{
        //    //        IncrementorEntryProcessor() {
        //    //            super(true);
        //    //        }
        //    public virtual object Process(DictionaryEntry entry)
        //    {
        //        int value = (int)entry.Value;
        //        if (value == null)
        //        {
        //            value = 0;
        //        }
        //        if (value == -1)
        //        {
        //            entry.SetValue(null);
        //            return null;
        //        }
        //        value++;
        //        entry.SetValue(value);
        //        return value;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public virtual void WriteData(ObjectDataOutput @out)
        //    {
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public virtual void ReadData(ObjectDataInput @in)
        //    {
        //    }

        //    public virtual void ProcessBackup(DictionaryEntry entry)
        //    {
        //        entry.SetValue((int)entry.Value + 1);
        //    }
        //}
	}
}
