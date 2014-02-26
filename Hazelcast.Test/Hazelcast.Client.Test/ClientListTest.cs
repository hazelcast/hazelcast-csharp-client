using System;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientListTest:HazelcastBaseTest
	{
		internal static IHList<object> list;

        [SetUp]
        public void Init()
        {

            list = client.GetList<object>(Name);
        }

        [TearDown]
        public static void Destroy()
        {
            list.Destroy();
        }

		[Test]
		public virtual void TestAddAll()
		{
			IList<object> l = new List<object>();
			l.Add("item1");
			l.Add("item2");
			Assert.IsTrue(list.AddAll(l));

			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list.AddAll(1, l));
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual("item1", list[0]);
			Assert.AreEqual("item1", list[1]);
			Assert.AreEqual("item2", list[2]);
			Assert.AreEqual("item2", list[3]);
		}

		[Test]
		public virtual void TestAddSetRemove()
		{
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item2"));
			list.Add(0, "item3");
			Assert.AreEqual(3, list.Count);
			object o = list.Set(2, "item4");
			Assert.AreEqual("item2", o);
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual("item3", list[0]);
			Assert.AreEqual("item1", list[1]);
			Assert.AreEqual("item4", list[2]);
			Assert.IsFalse(list.Remove("item2"));
			Assert.IsTrue(list.Remove("item3"));
			o = list.Remove(1);
			Assert.AreEqual("item4", o);
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("item1", list[0]);

		    list[0] = "itemMod";
            Assert.AreEqual("itemMod", list.Get(0));
		}
		[Test]
		public virtual void TestInsert()
		{
		    list.Add("item0");
		    list.Add("item1");
		    list.Add("item2");
            list.Insert(1,"item1Mod");
            Assert.AreEqual("item1Mod", list[1]);
            list.RemoveAt(0);
            Assert.AreEqual("item1Mod", list[0]);
            Assert.AreEqual("item1", list[1]);
		}

		[Test]
		public virtual void TestIndexOf()
		{
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item2"));
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item4"));
			Assert.AreEqual(-1, list.IndexOf("item5"));
			Assert.AreEqual(0, list.IndexOf("item1"));
			Assert.AreEqual(-1, list.LastIndexOf("item6"));
			Assert.AreEqual(2, list.LastIndexOf("item1"));
		}

		[Test]
		public virtual void TestIterator()
		{
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item2"));
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item4"));
		    var iter=list.GetEnumerator();
		    Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual("item1", iter.Current);
		    Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item2", iter.Current);
		    Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item1", iter.Current);
		    Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item4", iter.Current);
            Assert.IsFalse(iter.MoveNext());

            var l = list.SubList(1, 3);
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual("item2", l[0]);
            Assert.AreEqual("item1", l[1]);
		}

		[Test]
		public virtual void TestContains()
		{
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item2"));
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item4"));
			Assert.IsFalse(list.Contains("item3"));
			Assert.IsTrue(list.Contains("item2"));
			var l = new List<object>();
			l.Add("item4");
			l.Add("item3");
			Assert.IsFalse(list.ContainsAll(l));
			Assert.IsTrue(list.Add("item3"));
			Assert.IsTrue(list.ContainsAll(l));
		}

		[Test]
		public virtual void RemoveRetainAll()
		{
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item2"));
			Assert.IsTrue(list.Add("item1"));
			Assert.IsTrue(list.Add("item4"));
            var l = new List<object>();
			l.Add("item4");
			l.Add("item3");
			Assert.IsTrue(list.RemoveAll(l));
			Assert.AreEqual(3, list.Count);
			Assert.IsFalse(list.RemoveAll(l));
			Assert.AreEqual(3, list.Count);
			l.Clear();
			l.Add("item1");
			l.Add("item2");
			Assert.IsFalse(list.RetainAll(l));
			Assert.AreEqual(3, list.Count);
			l.Clear();
			Assert.IsTrue(list.RetainAll(l));
			Assert.AreEqual(0, list.Count);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestListener()
		{
			//        final ISet tempSet = server.getSet(name);
			var tempList = list;

            CountdownEvent latch = new CountdownEvent(6);

            var listener = new _ItemListener<object>(latch);
		    string registrationId = tempList.AddItemListener(listener, true);

            var t=new Thread(delegate(object o)
            {
                for (int i = 0; i < 5; i++)
                {
                    tempList.Add("item" + i);
                }
                tempList.Add("done"); 
            });
			t.Start();
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
		}

		internal sealed class _ItemListener<T> : IItemListener<T>
		{
			private readonly CountdownEvent latch;

		    public _ItemListener(CountdownEvent latch)
		    {
		        this.latch = latch;
		    }

		    public void ItemAdded(ItemEvent<T> item)
		    {
		        latch.Signal();
		    }

		    public void ItemRemoved(ItemEvent<T> item)
		    {
		    }
		}


	}
}
