using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Hazelcast.Core;


using Hazelcast.Client.IO;
namespace Hazelcast.Client
{
	public class TestClient
	{
		public static void Main2()
		{
			//ProtoSerializer serializer = new ProtoSerializer();
			//DefaultSerializer.register(serializer);
			
			HazelcastClient client = HazelcastClient.newHazelcastClient ("dev", "dev-pass", "localhost");
			//client.addInstanceListener(new MyInstanceListener());
			
			IMap<object, object> map = client.getMap<object, object>("default");
			IQueue<String> queue = client.getQueue<String>("default");
			ITopic<String> topic = client.getTopic<String>("default");
			
			Console.WriteLine("Putting");
			client.getCluster().getMembers();
			Console.WriteLine("Put Result: " + map.put("key", "value"));
			Console.WriteLine("Get Result: " + map.get("key"));
			Console.WriteLine("Remove Result: " + map.remove("key"));
			Console.WriteLine("Put Result: " + map.put("key", "value"));
			Console.WriteLine("Flush Map: "); map.flush();
			Console.WriteLine("Try Remove: " + map.tryRemove("key", 1000));
			Console.WriteLine("Try Put: " + map.tryPut("key", "value", 1000));
			Console.WriteLine("Put with ttl " + map.put("key", "value", 10000));
			Console.WriteLine("Put transient "); map.putTransient("key", "value", 10000);
			Console.WriteLine("Put if absent: " + map.putIfAbsent("key", "value", 10000));
			Console.WriteLine("Try Lock and Get: " + map.tryLockAndGet("key", 10000));
			Console.WriteLine("Put and unlock: ") ;map.putAndUnlock("key", "value1");
			Console.WriteLine("Try Lock lock: " + map.tryLock("key"));
			Console.WriteLine("Try Lock lock: " + map.tryLock("key", 3000));
			Console.WriteLine("Map lock: ") ; map.Lock("key");
			Console.WriteLine("Map unlock: ") ; map.unlock("key");
			Console.WriteLine("Locking Map: " + map.lockMap(1000));
			Console.WriteLine("UnLocking Map: "); map.unlockMap();
			Console.WriteLine("Evict Map: " + map.evict("key"));
			Console.WriteLine("Add index: "); map.addIndex("name1", false);
			//getAll(map);
			map.addEntryListener(new MyEntryListener<object, object>(),true);
			map.put("key", "value1");
			map.put("key", "value2");
			map.remove("key");
			queue.addItemListener(new MyItemListener<String>(), true);
			Console.WriteLine(queue.offer("1"));
			Console.WriteLine(queue.poll());
			
			DateTime start = System.DateTime.Now;
			
			for(int i=0;i<10000;i++){
				map.put(""+i, ""+i);
			}
			Console.WriteLine("Took " + (System.DateTime.Now-start));
			
			
			topic.addMessageListener(new MyMessageListener<String>());
			topic.publish("naber guduk");
			
			
			
			
			
			
			
						
			
			
			
			
			
 			
			
			
			//IMap<String, Employee2> employees = client.getMap<String, Employee2>("employees");
			//Console.WriteLine(employees.put("1", new Employee2(9)));
//			Console.WriteLine("Putting " + employees.put ("1", new Employee2(31)));
			//Console.WriteLine("Age is " + employees.get ("1").age);
			
			
//			IMap<String, int> nums = client.getMap<String, int>("nums");
//			nums.put("a", 1);
//			nums.put("2", 2);
//			nums.put("3", 3);
//			Console.WriteLine("Number read is " + nums.get("a"));
//			
			//byte[] bytes = IOUtil.toByte(new Employee2(9));
			
			//printBytes(bytes);
			//Employee2 deser = (Employee2)IOUtil.toObject(bytes);
			//Console.WriteLine(deser.age);
			//printBytes(IOUtil.toByte("value"));
			//printBytes(IOUtil.toByte("key"));
			
			//EmployeeProtos.Employee emp = new EmployeeProtos.Employee();
			//emp.email = "fuad@hazelcast.com";
			
			//Object employee = map.put(3, emp);
			//Console.WriteLine("Employee is: " + employee);
			
			//IMap<String, Employee2> employees = client.getMap<String, Employee2>("employees");
			//IMap<String, String> strs = client.getMap<String, String>("default");
			//DateTime start = DateTime.UtcNow;
			//for(int i=0;i<10000;i++){
			//	strs.put(""+i, ""+i);	
			//}
			
			//Console.WriteLine("Took " + (DateTime.UtcNow-start).TotalMilliseconds);
			
		}
		
		private static void printBytes (byte[] bytes)
		{
			foreach (byte b in bytes) {
				Console.Write (b);
				Console.Write (".");
			}
			//Console.WriteLine("Size is: " + bytes.Length);
		}
		
		
		private void getAll(IMap<object, object> map){
			HashSet<object> set = new HashSet<object>();
			set.Add("key1");
			set.Add("key3");
			
			map.put("key1", "value1");
			map.put("key2", "value2");
			map.put("key3", "value3");
			
			
			Console.WriteLine("Get all: "); 
			Dictionary<object, object> d = map.getAll(set);
			
			foreach (Object o in d.Values){
				Console.WriteLine(o);
			}
			
			foreach (Object o in d.Keys){
				Console.WriteLine(o);
			}

		}
	}
	public class MyMessageListener<E>:MessageListener<E>{
		public void onMessage<E>(Message<E> message){
			Console.WriteLine(message.getMessageObject());
		}
		
	}
	public class MyItemListener<E>: ItemListener<E>{
		public void itemAdded<E>(ItemEvent<E> item){
			Console.WriteLine("Item is added!" + item.Item);
		}
		public void itemRemoved<E>(ItemEvent<E> item){
			Console.WriteLine("Item is removed!" + item.Item);
		}		
	}
	
	public class MyEntryListener<K,V>: EntryListener<K,V>{
		public void entryAdded(EntryEvent<K, V> e){
			Console.WriteLine("Added" + e.Key + ": " + e.Value + ": "+ e.OldValue);	
		}
		public void entryRemoved(EntryEvent<K, V> e){
			Console.WriteLine("Removed" + e.Key + ": " + e.Value + ": "+ e.OldValue);
		}
		public void entryUpdated(EntryEvent<K, V> e){
			Console.WriteLine("Updated" + e.Key + ": " + e.Value + ": "+ e.OldValue);
		}
		public void entryEvicted(EntryEvent<K, V> e){
			Console.WriteLine("Evicted" + e.Key + ": " + e.Value + ": "+ e.OldValue);
		}	
	}
	
	class MyInstanceListener: InstanceListener{
			public MyInstanceListener (){
			
			}
			public void instanceCreated(InstanceEvent e){
				Console.WriteLine(e);
			}
	
		    public void instanceDestroyed(InstanceEvent e){
				Console.WriteLine(e);
			}
		}

	[Serializable()]
	public class Employee2 : ISerializable
	{
		public int age;
		
		public Employee2(int a){
			this.age = a;
		}
		
		public Employee2(){
			age = 5;
		}

		public Employee2 (SerializationInfo info, StreamingContext ctxt)
		{
			age = (int)info.GetValue ("age", typeof(int));
			//Console.WriteLine("DeSerializing " + age);
		}


		public void GetObjectData (SerializationInfo info, StreamingContext ctxt)
		{
			//Console.WriteLine("Serializing " + age);
			info.AddValue ("age", age);
		}
		
	}
}

