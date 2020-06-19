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

using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Sandbox
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public async Task Test()
        {
            var count = 0;

            var testing = new Testing();

            await testing.TriggerSomething(1).CAF();
            Assert.AreEqual(0, count);

            testing.OnSomething.Add(args => count += args);
            await testing.TriggerSomething(1).CAF();
            Assert.AreEqual(1, count);

            testing.OnSomething.Add(args =>
            {
                count += args;
                return default;
            });
            await testing.TriggerSomething(1).CAF();
            Assert.AreEqual(3, count);
        }

        public class Testing
        {
            /// <summary>
            /// Occurs when ...
            /// </summary>
            public MixedEvent<int> OnSomething { get; } = new MixedEvent<int>();

            public async ValueTask TriggerSomething(int args) => await OnSomething.InvokeAsync(args).CAF();
        }
    }
}
