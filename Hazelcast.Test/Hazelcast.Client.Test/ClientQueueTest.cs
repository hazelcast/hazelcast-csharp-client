using System;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientQueueTest:HazelcastBaseTest
	{
		internal const string queueName = "ClientQueueTest";

		internal static IQueue<object> q;

        [SetUp]
        public static void Init()
        {
            InitClient();
            q = client.GetQueue<object>(queueName);
        }

        [TearDown]
        public static void Destroy()
        {
            q.Clear();
        }

		//    @BeforeClass
		//    public static void init(){
		//        Config config = new Config();
		//        QueueConfig queueConfig = config.getQueueConfig(queueName);
		//        queueConfig.setMaxSize(6);
		//        server = Hazelcast.newHazelcastInstance(config);
		//        hz = HazelcastClient.newHazelcastClient(null);
		//        q = hz.getQueue(queueName);
		//    }
		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestListener()
		{
			Assert.AreEqual(0, q.Count);
			CountdownEvent latch = new CountdownEvent(5);
			var listener = new ClientListTest._ItemListener<object>(latch);
			string id = q.AddItemListener(listener, true);

            Thread.Sleep(500);

		    var t1 = new Thread(delegate(object o)
		    {
		        for (int i = 0; i < 5; i++)
		        {
		            if (!q.Offer("event_item" + i))
		            {
		                throw new SystemException();
		            }
		        }
		    });
			t1.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
			q.RemoveItemListener(id);
		}

		[Test]
		public virtual void TestOfferPoll()
		{
			for (int i = 0; i < 10; i++)
			{
				bool result = q.Offer("item");
				if (i < 6)
				{
					Assert.IsTrue(result);
				}
				else
				{
					Assert.IsFalse(result);
				}
			}
			Assert.AreEqual(6, q.Count);

            var t1 = new Thread(delegate(object o)
            {
                try
                {
                    Thread.Sleep(2 * 1000);
                }
                catch (Exception e)
                {
                }
                q.Poll();
            });
            t1.Start();

			bool result_1 = q.Offer("item", 5, TimeUnit.SECONDS);
			Assert.IsTrue(result_1);
			for (int i_1 = 0; i_1 < 10; i_1++)
			{
				object o = q.Poll();
				if (i_1 < 6)
				{
					Assert.IsNotNull(o);
				}
				else
				{
					Assert.IsNull(o);
				}
			}
			Assert.AreEqual(0, q.Count);


            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    Thread.Sleep(2 * 1000);
                }
                catch (Exception e)
                {
                }
                q.Offer("item1");
            });
            t2.Start();

            object o_1 = q.Poll(5, TimeUnit.SECONDS);
			Assert.AreEqual("item1", o_1);
			t1.Join(10000);
			t2.Join(10000);
		}



		/// <exception cref="System.IO.IOException"></exception>
		[Test]
		public virtual void TestRemainingCapacity()
		{
			Assert.AreEqual(6, q.RemainingCapacity());
			q.Offer("item");
			Assert.AreEqual(5, q.RemainingCapacity());
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Test]
		public virtual void TestRemove()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsFalse(q.Remove("item4"));
			Assert.AreEqual(3, q.Count);
			Assert.IsTrue(q.Remove("item2"));
			Assert.AreEqual(2, q.Count);
			Assert.AreEqual("item1", q.Poll());
			Assert.AreEqual("item3", q.Poll());
		}

		[Test]
		public virtual void TestContains()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
			Assert.IsTrue(q.Contains("item3"));
			Assert.IsFalse(q.Contains("item"));
			var list = new List<object>(2);
			list.Add("item4");
			list.Add("item2");
			Assert.IsTrue(q.ContainsAll(list));
			list.Add("item");
			Assert.IsFalse(q.ContainsAll(list));
		}

		[Test]
		public virtual void TestDrain()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
			var list = new List<object>();
			int result = q.DrainTo(list, 2);
			Assert.AreEqual(2, result);
			Assert.AreEqual("item1", list[0]);
			Assert.AreEqual("item2", list[1]);
            list = new List<object>();
			result = q.DrainTo(list);
			Assert.AreEqual(3, result);
			Assert.AreEqual("item3", list[0]);
			Assert.AreEqual("item4", list[1]);
			Assert.AreEqual("item5", list[2]);
		}

		[Test]
		public virtual void TestIterator()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
			int i = 0;
			foreach (object o in q)
			{
				i++;
				Assert.AreEqual("item" + i, o);
			}
		}

		[Test]
		public virtual void TestToArray()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
		    object[] array = q.ToArray();
			int i = 0;
			foreach (object o in array)
			{
				i++;
				Assert.AreEqual("item" + i, o);
			}
			object[] objects = q.ToArray(new object[2]);
			i = 0;
			foreach (object o_1 in objects)
			{
				i++;
				Assert.AreEqual("item" + i, o_1);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Test]
		public virtual void TestAddAll()
		{
			var coll = new List<object>();
			coll.Add("item1");
			coll.Add("item2");
			coll.Add("item3");
			coll.Add("item4");
			Assert.IsTrue(q.AddAll(coll));
			int size = q.Count;
			Assert.AreEqual(size, coll.Count);
		}

		[Test]
		public virtual void TestRemoveRetain()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
			var list = new List<object>();
			list.Add("item8");
			list.Add("item9");
			Assert.IsFalse(q.RemoveAll(list));
			Assert.AreEqual(5, q.Count);
			list.Add("item3");
			list.Add("item4");
			list.Add("item1");
			Assert.IsTrue(q.RemoveAll(list));
			Assert.AreEqual(2, q.Count);
			list.Clear();
			list.Add("item2");
			list.Add("item5");
			Assert.IsFalse(q.RetainAll(list));
			Assert.AreEqual(2, q.Count);
			list.Clear();
			Assert.IsTrue(q.RetainAll(list));
			Assert.AreEqual(0, q.Count);
		}

		[Test]
		public virtual void TestClear()
		{
			Assert.IsTrue(q.Offer("item1"));
			Assert.IsTrue(q.Offer("item2"));
			Assert.IsTrue(q.Offer("item3"));
			Assert.IsTrue(q.Offer("item4"));
			Assert.IsTrue(q.Offer("item5"));
			q.Clear();
			Assert.AreEqual(0, q.Count);
			Assert.IsNull(q.Poll());
		}
	}
}
