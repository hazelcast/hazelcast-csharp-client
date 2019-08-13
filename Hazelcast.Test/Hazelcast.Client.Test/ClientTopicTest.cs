// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    public class ClientTopicTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _t = Client.GetTopic<object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _t.Destroy();
        }


        ITopic<object> _t;

        sealed class MessageListener : IMessageListener<object>
        {
            readonly CountdownEvent _latch;

            public MessageListener(CountdownEvent latch)
            {
                _latch = latch;
            }

            public void OnMessage(Message<object> message)
            {
                _latch.Signal();
            }
        }


        [Test]
        public void Listener()
        {
            var latch = new CountdownEvent(10);
            var listener = new MessageListener(latch);
            var id = _t.AddMessageListener(listener);
            for (var i = 0; i < 10; i++)
            {
                _t.Publish("naber" + i);
            }
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void ListenerRemove()
        {
            var latch = new CountdownEvent(1);
            var listener = new MessageListener(latch);
            var id = _t.AddMessageListener(listener);
            Assert.IsTrue(_t.RemoveMessageListener(id));

            _t.Publish("naber");
            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(5)));
        }
    }
}