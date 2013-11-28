using System;
using System.Text;
using Hazelcast.Util;


namespace Hazelcast.Util
{
	
	public sealed class Clock
	{
		public static long CurrentTimeMillis()
		{
			return _clock.CurrentTimeMillis();
		}

		private static readonly ClockImpl _clock;

		static Clock()
		{
            //TODO com.hazelcast.clock.offset
		    string clockOffset = "0";//Runtime.GetProperty("com.hazelcast.clock.offset");
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
				_clock = new Clock.SystemClock();
			}
			else
			{
				_clock = new Clock.SystemOffsetClock(offset);
			}
		}

        internal abstract class ClockImpl
		{
			protected internal abstract long CurrentTimeMillis();
		}

		private sealed class SystemClock : Clock.ClockImpl
		{
			protected internal sealed override long CurrentTimeMillis()
			{
                return __CurrentTimeMillis;
			}

			public override string ToString()
			{
				return "SystemClock";
			}
		}

		internal sealed class SystemOffsetClock : Clock.ClockImpl
		{
			private readonly long offset;

            internal SystemOffsetClock(long offset)
			{
				this.offset = offset;
			}

			protected internal sealed override long CurrentTimeMillis()
			{
                return __CurrentTimeMillis +offset;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("SystemOffsetClock");
				sb.Append("{offset=").Append(offset);
				sb.Append('}');
				return sb.ToString();
			}
		}

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long __CurrentTimeMillis
        {
            get{return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;}
        }

		private Clock()
		{
		}
	}
}
