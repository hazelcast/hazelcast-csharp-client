// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class Batch
    {
        private readonly long _increment;
        private readonly long _max;
        private readonly DateTime _expires;

        private long _current;

        public Batch(long start, long increment, int count, TimeSpan timeout)
        {
            if (increment <= 0)
                throw new ArgumentException("Increment must be positive.", nameof(increment));
            if (count <= 0)
                throw new ArgumentException("Batch count must be positive.", nameof(count));
            if (timeout <= TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
                throw new ArgumentException("Timeout must be positive or infinite.", nameof(timeout));

            // a batch can provide <count> ids, starting with <start>, incrementing by <increment>
            // so: <start>, <start> + <increment>, ..., <start> + <increment> * (<count> - 1)

            _current = start - increment; // we will always increment before returning
            _increment = increment;
            _max = start + increment * (count - 1);

            _expires = timeout == Timeout.InfiniteTimeSpan
                ? DateTime.MaxValue
                : Clock.Now + timeout;
        }

        public bool TryGetNextId(out long id)
        {
            id = default;

            // fail if the batch has expired
            if (_expires != DateTime.MaxValue && _expires < Clock.Now)
                return false;

            // fail if the batch has been fully used
            if (_current > _max)
                return false;

            id = Interlocked.Add(ref _current, _increment);
            return id <= _max;
        }
    }
}
