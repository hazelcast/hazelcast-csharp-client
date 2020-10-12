// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Data;
using Hazelcast.Events;
using Hazelcast.Networking;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Events
{
    [TestFixture]
    public class EventHandlers
    {
        [Test]
        public async Task ClientLifecycleEventHandler()
        {
            var count = 0;
            object eventSender = null, eventArgs = null;

            var handler = new ClientLifecycleEventHandler((sender, args) =>
            {
                eventSender = sender;
                eventArgs = args;
                count++;
                return new ValueTask();
            });

            var client = Mock.Of<IHazelcastClient>();
            var args = new ClientLifecycleEventArgs(ClientLifecycleState.Shutdown);
            await handler.HandleAsync(client, args);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(client));
            Assert.That(eventArgs, Is.SameAs(args));
        }

        [Test]
        public async Task PartitionLostEventHandler()
        {
            var count = 0;
            object eventSender = null, eventArgs = null;

            var handler = new PartitionLostEventHandler((sender, args) =>
            {
                eventSender = sender;
                eventArgs = args;
                count++;
                return new ValueTask();
            });

            var client = Mock.Of<IHazelcastClient>();
            var memberInfo = new MemberInfo(Guid.NewGuid(), NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());
            var args = new PartitionLostEventArgs(12, 13, true, memberInfo);
            await handler.HandleAsync(client, args);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(client));
            Assert.That(eventArgs, Is.SameAs(args));
        }

        [Test]
        public async Task ConnectionLifecycleEventHandler()
        {
            var count = 0;
            object eventSender = null, eventArgs = null;

            var handler = new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, (sender, args) =>
            {
                eventSender = sender;
                eventArgs = args;
                count++;
                return new ValueTask();
            });

            Assert.That(handler.EventType, Is.EqualTo(ConnectionLifecycleEventType.Added));

            var client = Mock.Of<IHazelcastClient>();
            var args = new ConnectionLifecycleEventArgs();
            await handler.HandleAsync(client, args);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(client));
            Assert.That(eventArgs, Is.SameAs(args));
        }

        [Test]
        public void HazelcastClientEventHandlers()
        {
            var counts = new int[9];

            var handle = new HazelcastClientEventHandlers();
            handle
                .ClientStateChanged((sender, args) => counts[0]++)
                .ConnectionAdded((sender, args) => counts[1]++)
                .ConnectionRemoved((sender, args) => counts[2]++)
                .MemberAdded((sender, args) => counts[3]++)
                .MemberRemoved((sender, args) => counts[4]++)
                .ObjectCreated((sender, args) => counts[5]++)
                .ObjectDestroyed((sender, args) => counts[6]++)
                .PartitionLost((sender, args) => counts[7]++)
                .PartitionsUpdated((sender, args) => counts[8]++);

            var handlers = handle.ToList();
            Assert.That(handlers.Count, Is.EqualTo(9));

            foreach (var handler in handlers)
                handle.Remove(handler);

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.Zero);

            handle
                .ClientStateChanged((sender, args) => { counts[0]++; return new ValueTask(); })
                .ConnectionAdded((sender, args) => { counts[1]++; return new ValueTask(); })
                .ConnectionRemoved((sender, args) => { counts[2]++; return new ValueTask(); })
                .MemberAdded((sender, args) => { counts[3]++; return new ValueTask(); })
                .MemberRemoved((sender, args) => { counts[4]++; return new ValueTask(); })
                .ObjectCreated((sender, args) => { counts[5]++; return new ValueTask(); })
                .ObjectDestroyed((sender, args) => { counts[6]++; return new ValueTask(); })
                .PartitionLost((sender, args) => { counts[7]++; return new ValueTask(); })
                .PartitionsUpdated((sender, args) => { counts[8]++; return new ValueTask(); });

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.EqualTo(9));

            foreach (var handler in handlers)
                handle.Remove(handler);

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.Zero);
        }
    }
}
