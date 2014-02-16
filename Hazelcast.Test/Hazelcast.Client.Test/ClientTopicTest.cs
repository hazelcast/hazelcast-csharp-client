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
            t = client.GetTopic<object>(Name);
        }

        [TearDown]
        public static void Destroy()
        {
        }


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestListener()
		{
            CountdownEvent latch = new CountdownEvent(10);
			var listener = new _MessageListener(latch);
			t.AddMessageListener(listener);
			for (int i = 0; i < 10; i++)
			{
				t.Publish("naber" + i);
			}
		    Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
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
