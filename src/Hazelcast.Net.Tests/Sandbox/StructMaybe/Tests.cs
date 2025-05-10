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
using System;

namespace Hazelcast.Tests.Sandbox.StructMaybe
{
    public class Tests
    {
        public void Test()
        {
            var foo = Maybe.Some(123);
            if (foo.TryGetValue(out var value))
                Console.WriteLine(value);
        }

        public Maybe<int> GetMaybeInt1()
        {
            // implicit cast of T to Maybe<T>
            return 3;
        }

        public Maybe<int> GetMaybeInt2()
        {
            // implicit case of Maybe.None to Maybe<T>
            return Maybe.None;
        }
    }
}
