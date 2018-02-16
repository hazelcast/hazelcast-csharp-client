// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientNearCacheConfigTest
    {
        [Test]
        public void TestReadOnlyNearCacheConfig()
        {
            var config = new NearCacheConfig();
            var readOnly = config.GetAsReadOnly();

            var actions = new Action[]
            {
                () => readOnly.SetEvictionPolicy(TestSupport.RandomString()),
                () => readOnly.SetName(TestSupport.RandomString()),
                () => readOnly.SetInMemoryFormat(TestSupport.RandomString()),
                () => readOnly.SetInMemoryFormat(InMemoryFormat.Binary),
                () => readOnly.SetInvalidateOnChange(true),
                () => readOnly.SetMaxIdleSeconds(TestSupport.RandomInt()),
                () => readOnly.SetMaxSize(TestSupport.RandomInt()),
                () => readOnly.SetTimeToLiveSeconds(TestSupport.RandomInt())
            };

            foreach (var action in actions)
            {
                try
                {
                    action();
                    Assert.Fail("The config was not readonly.");
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheBeginning()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("*Map");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("*MapStudent");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("fooMap");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("fooMapStudent");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheEnd()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("map*");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("mapStudent*");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFoo");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFoo");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskInTheMiddle()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("map*Bar");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("mapStudent*Bar");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFooBar");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFooBar");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }
    }
}