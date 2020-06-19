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
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks
{
    // the purpose of this benchmark is to determine whether it is cheaper to async/await
    // and then dispose a disposable, or elide the async/await (and avoid the cost of the
    // associated state machine) and dispose in a continuation.
    //
    // results:
    //
    //  |             Method |     Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //  |------------------- |---------:|----------:|----------:|-------:|------:|------:|----------:|
    //  |       WithAwaiting | 1.884 us | 0.0148 us | 0.0138 us | 0.0896 |     - |     - |     383 B |
    //  |   WithContinuation | 2.105 us | 0.0386 us | 0.0361 us | 0.1259 |     - |     - |     530 B |
    //
    // conclusion:
    //
    // is is cheaper to pay for the async/await state machine that to use a continuation.

    public class DisposeWithAsyncAwaitOrContinuation
    {
        [Benchmark]
        public async Task WithAwaiting()
        {
            await RunAwaiting();
        }

        [Benchmark]
        public async Task WithContinuation()
        {
            await RunContinuation();
        }

        public async Task RunAwaiting()
        {
            // await the Whatever method, and *then* dispose the disposable
            // so we pay for the async/await state machine, but not for the continuation
            using var disposable = new SomeDisposable();
            await Whatever();
        }

        public Task RunContinuation()
        {
            // return the Whatever() task without awaiting, with a continuation
            // which will dispose the disposable once the task has completed,
            // so we pay for the continuation, but not for the async/await state machine
            var disposable = new SomeDisposable();
            return ThenDispose(Whatever(), disposable);

            // could be this with an extension method
            //return Whatever().ThenDispose(disposable);
        }

        public async Task Whatever()
        {
            await Task.Yield();
        }

        /// <summary>
        /// Configures a task to dispose a resource after it completes.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="disposable">The disposable resource.</param>
        /// <returns>A task.</returns>
        public static Task ThenDispose(Task task, IDisposable disposable)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return task.ContinueWith(x =>
            {
                disposable.Dispose();
                return x;
            }, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current).Unwrap();
        }

        public class SomeDisposable : IDisposable
        {
            public void Dispose()
            { }
        }
    }
}