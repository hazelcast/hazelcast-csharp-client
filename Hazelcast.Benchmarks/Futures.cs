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

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;

namespace Hazelcast.Benchmarks
{
    [CoreJob, ClrJob]
    [MemoryDiagnoser]
    public class Futures
    {
        static readonly IClientMessage Result = new ClientMessage();

        [Benchmark]
        public object SyncFuture()
        {
            var future = new SyncFuture<object>();
            ((IFuture<object>)future).SetResult(Result);
            return future.WaitAndGet();
        }

        [Benchmark]
        public async Task<object> AsyncFuture()
        {
            var future = AsyncFuture<object>.Create(out var tcs);
            future.SetResult(Result);
            return await tcs.Task;
        }
    }
}