// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
    internal class AtomicBoolean
    {
        private int _value;

        public AtomicBoolean()
        {
        }

        public AtomicBoolean(bool initialValue)
        {
            _value = initialValue ? 1 : 0;
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            var e = expect ? 1 : 0;
            var u = update ? 1 : 0;
            return (Interlocked.CompareExchange(ref _value, u, e) == e);
        }

        public bool Get()
        {
            return _value != 0;
        }

        public bool GetAndSet(bool newValue)
        {
            for (;;)
            {
                var current = Get();
                if (CompareAndSet(current, newValue))
                    return current;
            }
        }

        public void Set(bool newValue)
        {
            _value = newValue ? 1 : 0;
        }

        public bool WeakCompareAndSet(bool expect, bool update)
        {
            return CompareAndSet(expect, update);
        }
    }
}