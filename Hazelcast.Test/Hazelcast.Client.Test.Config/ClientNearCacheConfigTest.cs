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
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientNearCacheConfigTest
    {
        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheBeginning()
        {
            var clientConfig = new Configuration();
            var genericNearCacheConfig = new NearCacheConfig {Name = "*Map"};
            clientConfig.NearCacheConfigs.Add(genericNearCacheConfig.Name, genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig {Name = "*MapStudent"};
            clientConfig.NearCacheConfigs.Add(specificNearCacheConfig.Name, specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("fooMap");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("fooMapStudent");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheEnd()
        {
            var clientConfig = new Configuration();
            var genericNearCacheConfig = new NearCacheConfig {Name = "map*"};
            clientConfig.NearCacheConfigs.Add(genericNearCacheConfig.Name, genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig {Name = "mapStudent*"};
            clientConfig.NearCacheConfigs.Add(specificNearCacheConfig.Name, specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFoo");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFoo");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskInTheMiddle()
        {
            var clientConfig = new Configuration();
            var genericNearCacheConfig = new NearCacheConfig {Name = "map*Bar"};
            clientConfig.NearCacheConfigs.Add(genericNearCacheConfig.Name, genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig {Name = "mapStudent*Bar"};
            clientConfig.NearCacheConfigs.Add(specificNearCacheConfig.Name, specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFooBar");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFooBar");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }
    }
}