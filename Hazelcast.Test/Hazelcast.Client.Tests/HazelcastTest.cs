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
			ClientConfig config = new ClientConfig();
			config.addAddress("127.0.0.1:5701");
			return HazelcastClient.newHazelcastClient(config);		
		}
		
		public HazelcastTest ()
		{
		}
		
		
	}
}

