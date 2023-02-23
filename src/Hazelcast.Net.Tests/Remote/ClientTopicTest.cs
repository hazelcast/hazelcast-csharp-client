// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientTopicTest : SingleMemberClientRemoteTestBase
    {
        private const string TopicNameBase = "test1";

        [Test]
        public async Task TestListener()
        {
            var topic = await Client.GetTopicAsync<string>(TopicNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(topic);

            var eventsCount = 0;
            var sid = await topic.SubscribeAsync(handle => handle
                .Message((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            for (var i = 0; i < 10; i++)
            {
                await topic.PublishAsync("naber" + i);
            }

            await AssertEx.SucceedsEventually(() => Assert.That(eventsCount, Is.EqualTo(10)), 4000, 500);

            await topic.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestListenerRemove()
        {
            var topic = await Client.GetSetAsync<string>(TopicNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(topic);

            var eventsCount = 0;
            var sid = await topic.SubscribeAsync(handle => handle
                .ItemAdded((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            await topic.UnsubscribeAsync(sid);

            for (var i = 0; i < 10; i++)
            {
                await topic.AddAsync("naber" + i);
            }

            await Task.Delay(4000);
            Assert.That(eventsCount, Is.Zero);
        }
    }
}
