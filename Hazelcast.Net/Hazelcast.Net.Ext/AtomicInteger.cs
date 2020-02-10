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

using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicInteger
    {
        private int _val;

        public AtomicInteger()
        {
        }

        public AtomicInteger(int val)
        {
            _val = val;
        }

        public int AddAndGet(int addval)
        {
            return Interlocked.Add(ref _val, addval);
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref _val);
        }

        public int Get()
        {
            return _val;
        }

        public int GetAndAdd(int addval)
        {
            var res = Interlocked.Add(ref _val, addval);
            return res - addval;
        }

        public int GetAndIncrement()
        {
            for (;;)
            {
                var current = Get();
                var next = current + 1;
                if (CompareAndSet(current, next))
                {
                    return current;
                }
            }
        }

        public int GetAndSet(int newValue)
        {
            return Interlocked.Exchange(ref _val, newValue);
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _val);
        }

        public void Set(int newValue)
        {
            Interlocked.Exchange(ref _val, newValue);
        }

        public bool CompareAndSet(int expect, int update)
        {
            return Interlocked.CompareExchange(ref _val, update, expect) == expect;
        }
    }
}