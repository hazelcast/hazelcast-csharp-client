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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.HExecutorImpl;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class ExecutorTests
    {
        public class HelloExecutable : IExecutable<string>
        {
            public string Execute()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        [Ignore("Not implemented.")]
        public async Task Test()
        {
            var executor = new Executor(null, null, null, null, null);
            var result = await executor.ExecuteAsync(new HelloExecutable(), CancellationToken.None);
            Assert.AreEqual("hello", result);
        }
    }
}
