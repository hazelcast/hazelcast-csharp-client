using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientMultiMapTest:HazelcastBaseTest
	{
        internal const string name = "ClientMultiMapTest";

		internal static IMultiMap<object,object> mm;

        [SetUp]
        public void Init()
        {
            mm = client.GetMultiMap<object, object>(Name);
        }

        [TearDown]
        public static void Destroy()
        {
        }

		[Test]
		public virtual void TestPutGetRemove()
		{
			Assert.IsTrue(mm.Put("key1", "value1"));
			Assert.IsTrue(mm.Put("key1", "value2"));
			Assert.IsTrue(mm.Put("key1", "value3"));
			Assert.IsTrue(mm.Put("key2", "value4"));
			Assert.IsTrue(mm.Put("key2", "value5"));
			Assert.AreEqual(3, mm.ValueCount("key1"));
			Assert.AreEqual(2, mm.ValueCount("key2"));
			Assert.AreEqual(5, mm.Size());
			var coll = mm.Get("key1");
			Assert.AreEqual(3, coll.Count);
			coll = mm.Remove("key2");
			Assert.AreEqual(2, coll.Count);
			Assert.AreEqual(0, mm.ValueCount("key2"));
			Assert.AreEqual(0, mm.Get("key2").Count);
			Assert.IsFalse(mm.Remove("key1", "value4"));
			Assert.AreEqual(3, mm.Size());
			Assert.IsTrue(mm.Remove("key1", "value2"));
			Assert.AreEqual(2, mm.Size());
			Assert.IsTrue(mm.Remove("key1", "value1"));
			Assert.AreEqual(1, mm.Size());
		    IEnumerator<object> enumerator = mm.Get("key1").GetEnumerator();
		    enumerator.MoveNext();
		    Assert.AreEqual("value3", enumerator.Current);
		}

		[Test]
		public virtual void TestKeySetEntrySetAndValues()
		{
			Assert.IsTrue(mm.Put("key1", "value1"));
			Assert.IsTrue(mm.Put("key1", "value2"));
			Assert.IsTrue(mm.Put("key1", "value3"));
			Assert.IsTrue(mm.Put("key2", "value4"));
			Assert.IsTrue(mm.Put("key2", "value5"));
			Assert.AreEqual(2, mm.KeySet().Count);
			Assert.AreEqual(5, mm.Values().Count);
			Assert.AreEqual(5, mm.EntrySet().Count);
		}

		[Test]
		public virtual void TestContains()
		{
			Assert.IsTrue(mm.Put("key1", "value1"));
			Assert.IsTrue(mm.Put("key1", "value2"));
			Assert.IsTrue(mm.Put("key1", "value3"));
			Assert.IsTrue(mm.Put("key2", "value4"));
			Assert.IsTrue(mm.Put("key2", "value5"));
			Assert.IsFalse(mm.ContainsKey("key3"));
			Assert.IsTrue(mm.ContainsKey("key1"));
			Assert.IsFalse(mm.ContainsValue("value6"));
			Assert.IsTrue(mm.ContainsValue("value4"));
			Assert.IsFalse(mm.ContainsEntry("key1", "value4"));
			Assert.IsFalse(mm.ContainsEntry("key2", "value3"));
			Assert.IsTrue(mm.ContainsEntry("key1", "value1"));
			Assert.IsTrue(mm.ContainsEntry("key2", "value5"));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestListener()
		{
			CountdownEvent latch1Add = new CountdownEvent(8);
			CountdownEvent latch1Remove = new CountdownEvent(4);
			CountdownEvent latch2Add = new CountdownEvent(3);
			CountdownEvent latch2Remove = new CountdownEvent(3);
            EntryAdapter<object,object> listener1=new EntryAdapter<object, object>(
                delegate(EntryEvent<object, object> @event) { latch1Add.Signal(); },
                delegate(EntryEvent<object, object> @event) { latch1Remove.Signal(); }, 
                delegate(EntryEvent<object, object> @event) {  },
                delegate(EntryEvent<object, object> @event) {  }  ); 

            EntryAdapter<object,object> listener2=new EntryAdapter<object, object>(
                delegate(EntryEvent<object, object> @event) { latch2Add.Signal(); },
                delegate(EntryEvent<object, object> @event) { latch2Remove.Signal(); }, 
                delegate(EntryEvent<object, object> @event) {  },
                delegate(EntryEvent<object, object> @event) {  }  ); 
            
			mm.AddEntryListener(listener1, true);
			mm.AddEntryListener(listener2, "key3", true);

            Thread.Sleep(1000);

			mm.Put("key1", "value1");
			mm.Put("key1", "value2");
			mm.Put("key1", "value3");
			mm.Put("key2", "value4");
			mm.Put("key2", "value5");
			mm.Remove("key1", "value2");
			mm.Put("key3", "value6");
			mm.Put("key3", "value7");
			mm.Put("key3", "value8");
			mm.Remove("key3");
			Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(20)));
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLock()
		{
			mm.Lock("key1");

            CountdownEvent latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                if (!ClientMultiMapTest.mm.TryLock("key1"))
                {
                    latch.Signal();
                }
            });
            t.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
			mm.ForceUnlock("key1");
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLockTtl()
		{
			mm.Lock("key1", 3, TimeUnit.SECONDS);
            CountdownEvent latch = new CountdownEvent(2);
            var t = new Thread(delegate(object o)
            {
                if (!ClientMultiMapTest.mm.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (mm.TryLock("key1", 5, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			mm.ForceUnlock("key1");
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTryLock()
		{
			Assert.IsTrue(mm.TryLock("key1", 2, TimeUnit.SECONDS));
            CountdownEvent latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                try
                {
                    if (!ClientMultiMapTest.mm.TryLock("key1", 2, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t.Start();


			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsTrue(mm.IsLocked("key1"));


            CountdownEvent latch2 = new CountdownEvent(1);
            var t2 = new Thread(delegate(object o)
            {

                try
                {
                    if (mm.TryLock("key1", 20, TimeUnit.SECONDS))
                    {
                        latch2.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t2.Start();

			Thread.Sleep(1000);
			mm.Unlock("key1");


			Assert.IsTrue(latch2.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsTrue(mm.IsLocked("key1"));
			mm.ForceUnlock("key1");
		}


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestForceUnlock()
		{
			mm.Lock("key1");
			CountdownEvent latch = new CountdownEvent(1);

            var t = new Thread(delegate(object o)
            {
                mm.ForceUnlock("key1");
                latch.Signal();
            });
            t.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsFalse(mm.IsLocked("key1"));
		}

	}
}
