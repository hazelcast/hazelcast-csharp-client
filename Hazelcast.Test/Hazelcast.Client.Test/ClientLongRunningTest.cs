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
using Hazelcast.Core;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("enterprise")]
    [Ignore("")]
    public class ClientLongRunningTest : SingleMemberBaseTest
    {
        private IMap<object, object> _map;
        
        [SetUp]
        public void Init()
        {
            _map = Client.GetMap<object, object>(TestSupport.RandomString());
            FillMap();
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast_delay;
        }

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put("key" + i, "value" + i);
            }
        }

        [Test]
        public void TestLongRunning()
        {
            var starTicks = DateTime.Now.Ticks;
            var collection = _map.Values(Predicates.Sql("this == 'value5'"));
            
            var timeSpan = new TimeSpan(DateTime.Now.Ticks - starTicks);
            Assert.IsNotEmpty(collection);
            Assert.GreaterOrEqual(timeSpan.TotalSeconds, 300);
        }
    }
}