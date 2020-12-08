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

namespace Hazelcast.Examples.DotNet
{
    // ReSharper disable once UnusedMember.Global
    public class UnobservedExceptionExample : ExampleBase
    {
        public void Run()
        {
            // this example is expected to FAIL because of an unobserved exception

            var task = Task.Run(() => throw new Exception("unobserved bang!"));

            // wait for the task to complete
            // but don't await, that would observe!
            while (!task.IsCompleted) Thread.Sleep(100);
        }
    }
}
