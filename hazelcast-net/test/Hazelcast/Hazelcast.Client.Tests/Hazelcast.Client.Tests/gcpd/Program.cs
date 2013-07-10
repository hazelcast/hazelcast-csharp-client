using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Query;
using System.Diagnostics;
using NUnit.Framework;

namespace Hazelcast.Client.Tests.gcpd.HazelcastClientTest
{
	[TestFixture()]
	class Program
	{
		private static string mapName = "data-map";
		[Test]
		public static void Main()
		{
			DemonstrateOutOfMemoryException();
		}
		
		/// <summary>
		/// Demonstrates an OutOfMemoryException retrieving values from an IMap using a SqlPredicate.
		/// In reality the predicate string will differ between calls, butthe same chunk
		/// of data is retrieved here for simplicity.
		/// 
		/// On a machine with 4Gb RAM this typically fails around i = 24,000.
		/// </summary>
		private static void DemonstrateOutOfMemoryException()
		{
			int offset = 0;
			int length = 64000;
			
			HazelcastClient client = CreateClient();
			
			String predicateStr = String.Format("(fromBytes between {0} and {1}) or (toBytes between {0} and {1})", offset, offset + (length - 1));
			for (int i = 0; i < 1; i++)
			{
				IMap<Int32, DataWrapper> dataMap = client.getMap<Int32, DataWrapper>(mapName);
				
				Predicate p = new SqlPredicate(predicateStr);
				
				System.Collections.Generic.ICollection<DataWrapper> matches = dataMap.Values(p);

				foreach(DataWrapper dw in matches){
					Console.WriteLine(dw);

				}
			
			}
			
		}
		
		private static HazelcastClient CreateClient()
		{
			ClientConfig cfg = new ClientConfig();
			cfg.TypeConverter = new TypeConverter();
			cfg.addAddress("127.0.0.1:5701");
			
			return HazelcastClient.newHazelcastClient(cfg);
			
		}
	}
}
