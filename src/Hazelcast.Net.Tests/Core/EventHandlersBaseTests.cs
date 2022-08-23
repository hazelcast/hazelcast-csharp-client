// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Linq;
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
