using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class IdGeneratorTest: HazelcastTest
	{
	
		[Test]
	    public void idGenerator() {
	        HazelcastClient hClient = getHazelcastClient();
	        Hazelcast.Core.IdGenerator clientId = hClient.getIdGenerator("id");
	        int count = 10;
	
			List<long> list = new List<long>(); 
			
	        for (int i = 0; i < count; i++) {
	            long genId = clientId.newId();
				Assert.IsFalse(list.Contains(genId));
				list.Add(genId);
	        }
	    }
	}
}

