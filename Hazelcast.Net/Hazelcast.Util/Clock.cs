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
using System.Text;

namespace Hazelcast.Util
{
    internal sealed class Clock
    {
        private static readonly ClockImpl TheClock;
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static Clock()
        {
            //TODO com.hazelcast.clock.offset
            var clockOffset = "0"; //Runtime.GetProperty("com.hazelcast.clock.offset");
            var offset = 0L;
            if (clockOffset != null)
            {
                try
                {
                    offset = long.Parse(clockOffset);
                }
                catch (FormatException)
                {
                }
            }
            if (offset == 0L)
            {
                TheClock = new SystemClock();
            }
            else
            {
                TheClock = new SystemOffsetClock(offset);
            }
        }

        private Clock()
        {
        }

        public static long CurrentTimeMillis()
        {
            return TheClock.CurrentTimeMillis();
        }

        private static long SystemCurrentTimeMillis()
        {
            return (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }

        internal abstract class ClockImpl
        {
            protected internal abstract long CurrentTimeMillis();
        }

        private sealed class SystemClock : ClockImpl
        {
            public override string ToString()
            {
                return "SystemClock";
            }

            protected internal override long CurrentTimeMillis()
            {
                return SystemCurrentTimeMillis();
            }
        }

        internal sealed class SystemOffsetClock : ClockImpl
        {
            private readonly long _offset;

            internal SystemOffsetClock(long offset)
            {
                _offset = offset;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("SystemOffsetClock");
                sb.Append("{offset=").Append(_offset);
                sb.Append('}');
                return sb.ToString();
            }

            protected internal override long CurrentTimeMillis()
            {
                return SystemCurrentTimeMillis() + _offset;
            }
        }
    }
}