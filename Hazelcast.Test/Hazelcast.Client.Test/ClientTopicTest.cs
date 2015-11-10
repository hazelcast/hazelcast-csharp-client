// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTopicTest : HazelcastBaseTest
    {
        [SetUp]
        public void Init()
        {
            t = Client.GetTopic<object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            t.Destroy();
        }

        internal const string name = "test1";

        internal static ITopic<object> t;

        private sealed class _MessageListener : IMessageListener<object>
        {
            private readonly CountdownEvent latch;

            public _MessageListener(CountdownEvent latch)
            {
                this.latch = latch;
            }

            public void OnMessage(Message<object> message)
            {
                latch.Signal();
            }
        }


        [Test]
        public virtual void TestListener()
        {
            var latch = new CountdownEvent(10);
            var listener = new _MessageListener(latch);
            var id = t.AddMessageListener(listener);
            for (var i = 0; i < 10; i++)
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
            var id = t.AddMessageListener(listener);
            Assert.IsTrue(t.RemoveMessageListener(id));

            t.Publish("naber");
            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(5)));
        }
    }
}