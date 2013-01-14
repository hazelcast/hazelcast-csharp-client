using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Query;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;


namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class Program
	{
		[Test()]
		public void connect ()
		{
			ClientConfig clientConfig = new ClientConfig();
			clientConfig.GroupConfig.Name = "dev";
			clientConfig.GroupConfig.Password = "dev-pass";
			//clientConfig.TypeConverter = MyTypeConverter;
			
			HazelcastClient Hazelcast = HazelcastClient.newHazelcastClient(clientConfig);
			
			//Allmost all cluster operations that you can do with ordinary HazelcastInstance
			//Note that the Customer class must have Serializable attribute or implement Hazelcast.IO.DataSerializable
			IMap<String, Customer> mapCustomers = Hazelcast.getMap<string, Customer>("default");
			//object d = mapCustomers.get("11111");
			
			Transaction tran = Hazelcast.getTransaction();
			tran.begin();
			mapCustomers.put("11111", new Customer("Joe", "Smith"));
			mapCustomers.put("22222", new Customer("Ali", "Selam"));
			
			
			mapCustomers.put("33333", new Customer("Avi", "Noyan"));
			tran.commit();
			System.Collections.Generic.ICollection<Customer> colCustomers = mapCustomers.Values();
		}
	}
}
