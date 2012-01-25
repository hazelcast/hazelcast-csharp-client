using System;

namespace Hazelcast.Client.Tests
{
	public class HazelcastTest
	{
		static HazelcastClient client = null;
		
		public static HazelcastClient getHazelcastClient(){
			if(client==null){
				client = newHazelcastClient();
			}
			return client;
			
		}
		
		public static HazelcastClient newHazelcastClient(){
			return HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");		
		}
		
		public HazelcastTest ()
		{
		}
		
		
	}
}

