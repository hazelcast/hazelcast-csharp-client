﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Test;
using NUnit.Framework;

namespace Hazelcast.NearCache.Test
{
    [TestFixture]
    public class Post38NearCacheTest : BaseNearCacheTest
    {
        [SetUp]
        public void Init()
        {
            _map = Client.GetMap<object, object>("nearCachedMap-" + TestSupport.RandomString());
            var nc = GetNearCache(_map);
            Assert.AreEqual(typeof(NearCache), nc.GetType());
        }
    }
}