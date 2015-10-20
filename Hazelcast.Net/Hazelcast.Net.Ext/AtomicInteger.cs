/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicInteger
    {
        private int val;

        public AtomicInteger()
        {
        }

        public AtomicInteger(int val)
        {
            this.val = val;
        }

        public int GetAndSet(int newValue)
        {
            return Interlocked.Exchange(ref val, newValue);
        }

        public int GetAndAdd(int addval)
        {
            int res = Interlocked.Add(ref val, addval);
            return res - addval;
        }

        public int AddAndGet(int addval)
        {
            return Interlocked.Add(ref val, addval);
        }

        private bool CompareAndSet(int expect, int update)
        {
            return (Interlocked.CompareExchange(ref val, update, expect) == expect);
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref val);
        }

        public int Get()
        {
            return val;
        }

        public void Set(int newValue)
        {
            val = newValue;
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref val);
        }

        public int GetAndIncrement()
        {
            for (;;)
            {
                int current = Get();
                int next = current + 1;
                if (CompareAndSet(current, next))
                {
                    return current;
                }
            }
        }
    }
}