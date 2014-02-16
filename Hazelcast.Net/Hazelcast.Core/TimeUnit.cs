using Hazelcast.Core;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represent Time units
    /// </summary>
    public enum TimeUnit : long
    {
        MILLISECONDS = 1,
        SECONDS = 1000,
        MINUTES = 60*1000
    }
    internal static class TimeUnitExtensions
    {
        public static long Convert(this TimeUnit thisUnit, long duration, TimeUnit targetUnit)
        {
            return ((duration*(long) targetUnit)/(long) thisUnit);
        }

        public static long ToMillis(this TimeUnit thisUnit, long duration)
        {
            return (duration*(long) thisUnit);
        }
    }
}

