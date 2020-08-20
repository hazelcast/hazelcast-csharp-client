﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class ResourcesTests
    {
        // testing that the resources are indeed available
        // used to fail on some non-windows platforms
        //
        // see:
        // https://github.com/hazelcast/hazelcast-csharp-client/pull/148
        // https://github.com/microsoft/msbuild/issues/2221
        // https://github.com/hazelcast/hazelcast-csharp-client/blob/d14e2695f6737d438c2ff31f91c4f4a5b7c556ef/Hazelcast.Test/Properties/Resources.cs

        [Test]
        public void HazelcastNetResources()
        {
            var value = Hazelcast.Configuration.Binding.Resources.Error_CannotActivateAbstractOrInterface;
            Assert.IsNotNull(value);
            Assert.Greater(value.Trim().Length, 0);
            Console.WriteLine($"Value: {value}");
        }

        [Test]
        public void HazelcastTestingResources()
        {
            var value = Hazelcast.Testing.Remote.Resources.hazelcast;
            Assert.IsNotNull(value);
            Assert.Greater(value.Trim().Length, 0);
            Console.WriteLine($"Value: {value}");
        }

        [Test]
        public void HazelcastTestsResources()
        {
            var value = Resources.EmptyWithComments;
            Assert.IsNotNull(value);
            Assert.Greater(value.Trim().Length, 0);
            Console.WriteLine($"Value: {value}");
        }
    }
}
