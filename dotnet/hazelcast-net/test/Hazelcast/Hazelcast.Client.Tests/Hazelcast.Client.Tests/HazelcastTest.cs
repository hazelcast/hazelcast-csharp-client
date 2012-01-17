using System;

namespace Hazelcast.Client.Tests
{
	public class HazelcastTest
	{
		static HazelcastClient client = null;
		
		public static HazelcastClient getHazelcastClient(){
			if(client==null){
				client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");		
			}
			return client;
			
		}
		
		public HazelcastTest ()
		{
		}
		
		
	}
}

