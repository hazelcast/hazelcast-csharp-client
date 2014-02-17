using System;
using System.Text;

namespace Hazelcast.Util
{
    internal sealed class Clock
    {
        private static readonly ClockImpl _clock;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static Clock()
        {
            //TODO com.hazelcast.clock.offset
            string clockOffset = "0"; //Runtime.GetProperty("com.hazelcast.clock.offset");
            long offset = 0L;
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
                _clock = new SystemClock();
            }
            else
            {
                _clock = new SystemOffsetClock(offset);
            }
        }

        private Clock()
        {
        }

        public static long __CurrentTimeMillis
        {
            get { return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds; }
        }

        public static long CurrentTimeMillis()
        {
            return _clock.CurrentTimeMillis();
        }

        internal abstract class ClockImpl
        {
            protected internal abstract long CurrentTimeMillis();
        }

        private sealed class SystemClock : ClockImpl
        {
            protected internal override long CurrentTimeMillis()
            {
                return __CurrentTimeMillis;
            }

            public override string ToString()
            {
                return "SystemClock";
            }
        }

        internal sealed class SystemOffsetClock : ClockImpl
        {
            private readonly long offset;

            internal SystemOffsetClock(long offset)
            {
                this.offset = offset;
            }

            protected internal override long CurrentTimeMillis()
            {
                return __CurrentTimeMillis + offset;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("SystemOffsetClock");
                sb.Append("{offset=").Append(offset);
                sb.Append('}');
                return sb.ToString();
            }
        }
    }
}