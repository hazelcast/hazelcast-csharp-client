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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientCountDownLatchTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            l = Client.GetCountDownLatch(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            l.Destroy();
        }

        private ICountDownLatch l;

        [Test]
        public void Latch()
        {
            const int count = 20;

            Assert.IsTrue(l.TrySetCount(count));
            Assert.IsFalse(l.TrySetCount(10));
            Assert.AreEqual(count, l.GetCount());

            TestSupport.Run(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    l.CountDown();
                    try
                    {
                        Thread.Sleep(60);
                    }
                    catch
                    {
                    }
                }
            });

            Assert.IsFalse(l.Await(1, TimeUnit.Seconds));
            Assert.IsTrue(l.Await(5, TimeUnit.Seconds));
        }
    }
}