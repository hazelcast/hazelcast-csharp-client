// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SequenceTests
    {
        [Test]
        public void Int32Sequence()
        {
            ISequence<int> sequence = new Int32Sequence();

            for (var i = 0; i < 100; i++)
                Assert.That(sequence.GetNext(), Is.EqualTo(i+1));
        }

        [Test]
        public void Int64Sequence()
        {
            ISequence<long> sequence = new Int64Sequence();

            for (var i = 0; i < 100; i++)
                Assert.That(sequence.GetNext(), Is.EqualTo(i+1));
        }
    }
}
