// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class StreamAsyncTests
    {
        [Test]
        [Timeout(10_000)]
        public async Task CanCancelRead()
        {
            Stream stream = new MemoryStream();

            var memory = new Memory<byte>(new byte[256]);

            var source = new CancellationTokenSource();
            //source.CancelAfter(2000);

            // note: a memory stream is non blocking!
            var count = await stream.ReadAsync(memory, source.Token).CfAwait();
            Console.WriteLine(count);

            // should end ok
        }
    }
}
