// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.FlakeId
{
    internal class Batch
    {
        private readonly long _increment;
        private readonly long _max;
        private readonly DateTime? _invalidSince;

        private long _current;

        public Batch(long @base, long increment, int batchSize, TimeSpan validityPeriod)
        {
            if (increment <= 0)
                throw new ArgumentException(@"Increment must be positive.", nameof(increment));
            if (batchSize <= 0)
                throw new ArgumentException(@"Batch size must be positive.", nameof(batchSize));
            if (validityPeriod <= TimeSpan.Zero && validityPeriod != Timeout.InfiniteTimeSpan)
                throw new ArgumentException(@"Validity period must be positive or infinite.", nameof(validityPeriod));

            _current = @base;
            _increment = increment;
            _max = @base + increment * (batchSize - 1);

            _invalidSince = validityPeriod == Timeout.InfiniteTimeSpan
                ? (DateTime?)null
                : DateTime.UtcNow + validityPeriod;
        }

        public bool TryGetNextId(out long id)
        {
            id = default;
            if (_invalidSince != null && DateTime.UtcNow >= _invalidSince)
                return false;
            if (_current > _max)
                return false;

            id = Interlocked.Add(ref _current, _increment) - _increment;
            return id <= _max;
        }
    }
}
