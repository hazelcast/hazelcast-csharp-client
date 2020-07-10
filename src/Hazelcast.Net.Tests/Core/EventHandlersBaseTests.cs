using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class EventHandlersBaseTests
    {
        [Test]
        public void Test()
        {
            var eventHandlers = new EventHandlers()
                .OnEvent1()
                .OnEvent2();

            var handlers = eventHandlers.ToList();

            Assert.That(handlers.Count, Is.EqualTo(2));
            Assert.That(handlers[0].Id, Is.EqualTo(1));
            Assert.That(handlers[1].Id, Is.EqualTo(2));

            handlers.Clear();
            var e = ((IEnumerable) eventHandlers).GetEnumerator();
            while (e.MoveNext())
                handlers.Add((Handler) e.Current);

            Assert.That(handlers.Count, Is.EqualTo(2));
            Assert.That(handlers[0].Id, Is.EqualTo(1));
            Assert.That(handlers[1].Id, Is.EqualTo(2));
        }

        private class EventHandlers : EventHandlersBase<Handler>
        {
            public EventHandlers OnEvent1()
            {
                Add(new Handler(1));
                return this;
            }

            public EventHandlers OnEvent2()
            {
                Add(new Handler(2));
                return this;
            }
        }

        private class Handler
        {
            public Handler(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }
    }
}
