using System;
using System.Collections;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using System.Collections.Generic;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientSetTest:HazelcastBaseTest
	{
        //internal const string name = "test";

		internal static IHSet<object> set;

       [SetUp]
        public void Init()
        {
            set = Client.GetSet<object>(Name);
        }

        [TearDown]
        public static void Destroy()
        {
            set.Clear();
        }

		[Test]
		public virtual void TestAddAll()
		{
			var l = new List<object>();
			l.Add("item1");
			l.Add("item2");
            Assert.IsTrue(set.AddAll(l));
			Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.AddAll( l));
			Assert.AreEqual(2, set.Count);
		}

		[Test]
		public virtual void TestAddRemove()
		{
			Assert.IsTrue(set.Add("item1"));
			Assert.IsTrue(set.Add("item2"));
			Assert.IsTrue(set.Add("item3"));
			Assert.AreEqual(3, set.Count);
			Assert.IsFalse(set.Add("item3"));
			Assert.AreEqual(3, set.Count);
			Assert.IsFalse(set.Remove("item4"));
			Assert.IsTrue(set.Remove("item3"));
		}

		[Test]
		public virtual void TestIterator()
		{
			Assert.IsTrue(set.Add("item1"));
			Assert.IsTrue(set.Add("item2"));
			Assert.IsTrue(set.Add("item3"));
			Assert.IsTrue(set.Add("item4"));
			IEnumerator iter = set.GetEnumerator();

		    iter.MoveNext();
			Assert.IsTrue(((string)iter.Current).StartsWith("item"));
		    iter.MoveNext();
			Assert.IsTrue(((string)iter.Current).StartsWith("item"));
		    iter.MoveNext();
			Assert.IsTrue(((string)iter.Current).StartsWith("item"));
		    iter.MoveNext();
			Assert.IsTrue(((string)iter.Current).StartsWith("item"));
            Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public virtual void TestContains()
		{
			Assert.IsTrue(set.Add("item1"));
			Assert.IsTrue(set.Add("item2"));
			Assert.IsTrue(set.Add("item3"));
			Assert.IsTrue(set.Add("item4"));
			Assert.IsFalse(set.Contains("item5"));
			Assert.IsTrue(set.Contains("item2"));
			var l = new List<object>();
			l.Add("item6");
			l.Add("item3");
			Assert.IsFalse(set.ContainsAll(l));
			Assert.IsTrue(set.Add("item6"));
			Assert.IsTrue(set.ContainsAll(l));
		}

		[Test]
		public virtual void RemoveRetainAll()
		{
			Assert.IsTrue(set.Add("item1"));
			Assert.IsTrue(set.Add("item2"));
			Assert.IsTrue(set.Add("item3"));
			Assert.IsTrue(set.Add("item4"));
			var l = new List<object>();
			l.Add("item4");
			l.Add("item3");
			Assert.IsTrue(set.RemoveAll(l));
			Assert.AreEqual(2, set.Count);
			Assert.IsFalse(set.RemoveAll(l));
			Assert.AreEqual(2, set.Count);
			l.Clear();
			l.Add("item1");
			l.Add("item2");
			Assert.IsFalse(set.RetainAll(l));
			Assert.AreEqual(2, set.Count);
			l.Clear();
			Assert.IsTrue(set.RetainAll(l));
			Assert.AreEqual(0, set.Count);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestListener()
		{
            ////        final ISet tempSet = server.getSet(name);
            //ISet tempSet = set;
            //CountDownLatch latch = new CountDownLatch(6);
            //ItemListener listener = new _ItemListener_149(latch);
            //string registrationId = tempSet.AddListener(listener, true);
            //new _Thread_160(tempSet).Start();
            //Assert.IsTrue(latch.Await(20, TimeUnit.Seconds));

            var tempSet = set;

            CountdownEvent latch = new CountdownEvent(6);

            var listener = new ClientListTest._ItemListener<object>(latch);
            string registrationId = tempSet.AddItemListener(listener, true);

            var t = new Thread(delegate(object o)
            {
                for (int i = 0; i < 5; i++)
                {
                    tempSet.Add("item" + i);
                }
                tempSet.Add("done");
            });
            t.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
		}

		[Test]
		public virtual void TestRemoveListener()
		{
            var tempSet = set;
            CountdownEvent latch = new CountdownEvent(1);

            var listener = new ClientListTest._ItemListener<object>(latch);
            string registrationId = tempSet.AddItemListener(listener, true);

		    Assert.IsTrue(tempSet.RemoveItemListener(registrationId));

            var t = new Thread(o => tempSet.Add("item"));
            t.Start();
            
            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(10)));
		}

	}
}
