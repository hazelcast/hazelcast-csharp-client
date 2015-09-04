using System;
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTopicTest:HazelcastBaseTest
	{
		internal const string name = "test1";

		internal static ITopic<object> t;

        [SetUp]
        public void Init()
        {
            t = Client.GetTopic<object>(Name);
        }

        [TearDown]
        public static void Destroy()
        {
            t.Destroy();
        }


		[Test]
		public virtual void TestListener()
		{
            CountdownEvent latch = new CountdownEvent(10);
			var listener = new _MessageListener(latch);
		    string id = t.AddMessageListener(listener);
		    for (int i = 0; i < 10; i++)
			{
				t.Publish("naber" + i);
			}
		    Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
		}
        
        [Test]
		public virtual void TestListenerRemove()
		{
            var latch = new CountdownEvent(1);
			var listener = new _MessageListener(latch);
		    string id = t.AddMessageListener(listener);
            Thread.Sleep(1000);
		    Assert.IsTrue(t.RemoveMessageListener(id));
            Thread.Sleep(1000);

            t.Publish("naber");
		    Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(5)));
		}

		private sealed class _MessageListener : IMessageListener<object>
		{
			public _MessageListener(CountdownEvent latch)
			{
				this.latch = latch;
			}

			public void OnMessage(Message<object> message)
			{
				latch.Signal();
			}

			private readonly CountdownEvent latch;
		}
	}
}
