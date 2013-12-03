using System;
using NUnit.Framework;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class ConnectionTest : HazelcastTest
	{
		[Test()]
		public void connect ()
		{
			HazelcastClient hClient = getHazelcastClient();
			Assert.IsTrue(true);
		}
	}
}

