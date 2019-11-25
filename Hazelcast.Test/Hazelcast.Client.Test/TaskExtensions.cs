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

using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    static class TaskExtensions
    {
        public static async Task ShouldThrow<T>(this Task task)
            where T : Exception
        {
            try
            {
                await task;
            }
            catch (T )
            {
                return;
            }

            Assert.Fail($"The exception of type '{typeof(T)}' should have been thrown but wasn't.");
        }
    }
}