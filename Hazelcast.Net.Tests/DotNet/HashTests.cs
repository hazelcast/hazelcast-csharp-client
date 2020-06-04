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

using System.Collections.Generic;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class HashTests
    {
        [Test]
        public void Test()
        {
            var d = new Dictionary<Thing, string>();
            d[new Thing(1)] = "a";
            d[new Thing(2)] = "b";
            d[new Thing(1)] = "c";
            Assert.AreEqual(2, d.Count);

            // the actual key is the hash of the object
        }

        public class Thing
        {
            public Thing(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public override int GetHashCode()
            {
                //return base.GetHashCode();
                return Value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Thing other)) return false;
                return other.Value == Value;
            }
        }
    }
}
