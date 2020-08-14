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
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;

namespace Hazelcast.Benchmarks
{
    // the purpose of this benchmark is to determine whether it is cheaper to capture arguments,
    // or pass them as arguments, as well as to evaluate the cost of the async/await state machine.
    //
    // results:
    //
    //  |            Method |     Mean |     Error |    StdDev |   Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //  |------------------ |---------:|----------:|----------:|---------:|-------:|------:|------:|----------:|
    //  |    ArgsAsyncAwait | 2.158 us | 0.0426 us | 0.0871 us | 2.127 us | 0.1373 |     - |     - |     575 B |
    //  |              Args | 1.952 us | 0.0367 us | 0.0377 us | 1.943 us | 0.1068 |     - |     - |     447 B |
    //  | CaptureAsyncAwait | 2.182 us | 0.0427 us | 0.0399 us | 2.186 us | 0.1411 |     - |     - |     599 B |
    //  |           Capture | 2.005 us | 0.0401 us | 0.1070 us | 1.961 us | 0.1106 |     - |     - |     471 B |
    //
    // conclusions:
    //
    // as expected, the async/await has a cost that is better avoided. and then, passing arguments is
    // more efficient than capturing.
    //
    // note that inspecting the IL shows that the compiled seems clever enough to merge the state machine
    // and the capture objects when both are used, but still, it needs to manage a state

    public class ArgsVsCaptureAndStateMachine
    {
        [Benchmark]
        public async Task ArgsAsyncAwait() => await RunArgsAsyncAwait(1, 1);

        [Benchmark]
        public async Task Args() => await RunArgs(1, 1);

        [Benchmark]
        public async Task CaptureAsyncAwait() => await RunCaptureAsyncAwait(1, 1);

        [Benchmark]
        public async Task Capture() => await RunCapture(1, 1);

        // runs with arguments + async/await state machine
        public async Task<int> RunArgsAsyncAwait(int i, int j)
            => await Run(F, i, j);

        // runs with arguments + no async/await state machine
        public Task<int> RunArgs(int i, int j)
            => Run(F, i, j);

        // runs with a capture + async/await state machine
        public async Task<int> RunCaptureAsyncAwait(int i, int j)
            => await Run((token) => F(i, j, token));

        // runs with a capture + no async/await state machine
        public Task<int> RunCapture(int i, int j)
            => Run((token) => F(i, j, token));

        private Task<int> Run(Func<CancellationToken, Task<int>> f)
            => f(default);

        private Task<int> Run(Func<int, int, CancellationToken, Task<int>> f, int x, int y)
            => f(x, y, default);

        // just a dummy async method
        public async Task<int> F(int i, int j, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return 3;
        }
    }
}
