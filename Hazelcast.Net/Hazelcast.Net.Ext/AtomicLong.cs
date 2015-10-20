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

using System.Runtime.CompilerServices;
using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal class AtomicLong
    {
        private long val;

        public AtomicLong()
        {
        }

        public AtomicLong(long val)
        {
            this.val = val;
        }

        public long AddAndGet(long addval)
        {
            return Interlocked.Add(ref val, addval);
        }

        public bool CompareAndSet(long expect, long update)
        {
            return (Interlocked.CompareExchange(ref val, update, expect) == expect);
        }

        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref val);
        }

        public long Get()
        {
            return val;
        }

        public void Set(long newValue)
        {
            val = newValue;
        }

        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref val);
        }

        public long GetAndSet(int i)
        {
            return Interlocked.Exchange(ref val, i);
        }
    }
}