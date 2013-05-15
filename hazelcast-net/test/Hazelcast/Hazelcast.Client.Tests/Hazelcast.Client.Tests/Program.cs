using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Hazelcast.Query;
using Hazelcast.Client;
using Hazelcast.Client.IO;
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;


namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class Program
	{
		[Test()]
		public void text ()
		{
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream, Encoding.UTF8);


			String s = "";
			for(int i=0;i<5000;i++){
				s+=i+".";
			}
			IOUtil.writeUTF(writer, s);
//			writer.Write(s);
//			writer.Write((short)290);

			Console.WriteLine("Length is " + stream.Length);
			for(int i=0;i<stream.Length;i++){
				Console.Write (stream.GetBuffer()[i] + ".");
			}
			Console.WriteLine();

			Console.WriteLine("7BitEncoded is");
			Write7BitEncodedInt(290);
		}

		protected void Write7BitEncodedInt (int value)
		{
			do
			{
				int num = value >> 7 & 33554431;
				byte b = (byte)(value & 127);
				if (num != 0)
				{
					b |= 128;
				}
				Console.Write(b);
				Console.Write(".");
				value = num;
			}
			while (value != 0);
		}

		[Test()]
		public void connect ()
		{

			ClientConfig clientConfig = new ClientConfig();
			clientConfig.GroupConfig.Name = "dev";
			clientConfig.GroupConfig.Password = "dev-pass";
			clientConfig.addAddress("127.0.0.1");
			//clientConfig.TypeConverter = MyTypeConverter;


			String s = "";
			for(int i=0;i<5000;i++){
				s += ("."+i);
			}
			Console.WriteLine("String is " + s);

			HazelcastClient Hazelcast = HazelcastClient.newHazelcastClient(clientConfig);
			IMap<String, String> map = Hazelcast.getMap<String, String>("default");
			map.put("1", s);
			String val = map.get("1");
			Console.WriteLine(val.Equals(s) + " Value is  " + val);
//			sw.Reset();
//			sw.Start();
//			//Allmost all cluster operations that you can do with ordinary HazelcastInstance
//			//Note that the Customer class must have Serializable attribute or implement Hazelcast.IO.DataSerializable
//			IMap<String, Customer> mapCustomers = Hazelcast.getMap<string, Customer>("default");
//			//object d = mapCustomers.get("11111");
//			
//			Transaction tran = Hazelcast.getTransaction();
//			tran.begin();
//			mapCustomers.put("11111", new Customer("Joe", "Smith"));
//			mapCustomers.put("22222", new Customer("Ali", "Selam"));
//			sw.Stop();
//			Console.WriteLine("Time elapsed for two puts is  " + sw.ElapsedMilliseconds);
//			
//			mapCustomers.put("33333", new Customer("Avi", "Noyan"));
//			tran.commit();
//			System.Collections.Generic.ICollection<Customer> colCustomers = mapCustomers.Values();
		}
	}
}
