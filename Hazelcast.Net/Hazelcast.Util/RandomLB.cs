using System;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Util
{
	/// <summary>The RandomLB randomly selects a member to route to.</summary>
	/// <remarks>The RandomLB randomly selects a member to route to.</remarks>
	public class RandomLB : AbstractLoadBalancer
	{
        private readonly Random random = new Random((int)new DateTime().Ticks);

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
