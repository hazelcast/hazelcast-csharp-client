using System;
using System.Threading.Tasks;
using Hazelcast.Eventing;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public async Task Test()
        {
            var count = 0;

            var testing = new Testing();

            await testing.TriggerSomething(1);
            Assert.AreEqual(0, count);

            testing.OnSomething.Add(args => count += args);
            await testing.TriggerSomething(1);
            Assert.AreEqual(1, count);

            testing.OnSomething.Add(args =>
            {
                count += args;
                return default;
            });
            await testing.TriggerSomething(1);
            Assert.AreEqual(3, count);
        }

        public class Testing
        {
            /// <summary>
            /// Occurs when ...
            /// </summary>
            public MixedEvent<int> OnSomething { get; } = new MixedEvent<int>();

            public async ValueTask TriggerSomething(int args) => await OnSomething.InvokeAsync(args);
        }
    }
}
