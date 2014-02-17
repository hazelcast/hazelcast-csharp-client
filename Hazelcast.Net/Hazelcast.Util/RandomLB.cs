using System;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    /// <summary>The RandomLB randomly selects a member to route to.</summary>
    /// <remarks>The RandomLB randomly selects a member to route to.</remarks>
    internal class RandomLB : AbstractLoadBalancer
    {
        private readonly Random random = new Random((int) new DateTime().Ticks);

        public override IMember Next()
        {
            if (Members == null || Members.Length == 0)
            {
                return null;
            }
            return Members[random.Next(Members.Length)];
        }
    }
}