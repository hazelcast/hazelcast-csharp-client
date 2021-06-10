// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Events;
using Hazelcast.Models;
using Hazelcast.Networking;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Events
{
    [TestFixture]
    public class EventHandlers
    {
        [Test]
        public async Task StateChangedEventHandler()
        {
            var count = 0;
            object eventSender = null, eventArgs = null;

            var handler = new StateChangedEventHandler((sender, args) =>
            {
                eventSender = sender;
                eventArgs = args;
                count++;
                return new ValueTask();
            });

            var client = Mock.Of<IHazelcastClient>();
            var args = new StateChangedEventArgs(ClientState.Connected);
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

            var handler = new ConnectionOpenedEventHandler((sender, args) =>
            {
                eventSender = sender;
                eventArgs = args;
                count++;
                return default;
            });

            var client = Mock.Of<IHazelcastClient>();
            var args = new ConnectionOpenedEventArgs(null, false);
            await handler.HandleAsync(client, args);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(client));
            Assert.That(eventArgs, Is.SameAs(args));
        }

        [Test]
        public void HazelcastClientEventHandlers()
        {
            var counts = new int[8];

            var handle = new HazelcastClientEventHandlers();
            handle
                .StateChanged((sender, args) => counts[0]++)
                .ConnectionOpened((sender, args) => counts[1]++)
                .ConnectionClosed((sender, args) => counts[2]++)
                .MembersUpdated((sender, args) => counts[3]++)
                .ObjectCreated((sender, args) => counts[4]++)
                .ObjectDestroyed((sender, args) => counts[5]++)
                .PartitionLost((sender, args) => counts[6]++)
                .PartitionsUpdated((sender, args) => counts[7]++);

            var handlers = handle.ToList();
            Assert.That(handlers.Count, Is.EqualTo(8));

            foreach (var handler in handlers)
                handle.Remove(handler);

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.Zero);

            handle
                .StateChanged((sender, args) => { counts[0]++; return new ValueTask(); })
                .ConnectionOpened((sender, args) => { counts[1]++; return new ValueTask(); })
                .ConnectionClosed((sender, args) => { counts[2]++; return new ValueTask(); })
                .MembersUpdated((sender, args) => { counts[3]++; return new ValueTask(); })
                .ObjectCreated((sender, args) => { counts[4]++; return new ValueTask(); })
                .ObjectDestroyed((sender, args) => { counts[5]++; return new ValueTask(); })
                .PartitionLost((sender, args) => { counts[6]++; return new ValueTask(); })
                .PartitionsUpdated((sender, args) => { counts[7]++; return new ValueTask(); });

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.EqualTo(8));

            foreach (var handler in handlers)
                handle.Remove(handler);

            handlers = handle.ToList();
            Assert.That(handlers.Count, Is.Zero);
        }
    }
}
