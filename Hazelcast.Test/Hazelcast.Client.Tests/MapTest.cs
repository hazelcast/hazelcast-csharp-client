using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Hazelcast.Client;
using System.Threading;
using Hazelcast.Client.IO;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Query;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class MapTest : HazelcastTest
	{
		[Test()]
		public void TestCase ()
		{
			
		}
		
		[Test()]
	    public void getMapName()
		{
	        HazelcastClient hClient = getHazelcastClient();
	        IMap<Object, Object> map = hClient.getMap<Object, Object>("getMapName");
	        Assert.AreEqual("getMapName", map.getName());
	    }
		
		[Test()]
    	public void lockMapKey()
		{
        	HazelcastClient hClient = getHazelcastClient();
	        IMap<String, String> map = hClient.getMap<String, String>("lockMapKey");
	        
	        map.put("a", "b");
	        Thread.Sleep(10);
	        map.Lock("a");
			bool done = false;
			
			
			CountdownEvent count = new CountdownEvent(1);
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				map.Lock("a");
				done = true;		
				count.Signal();
				map.unlock("a");
					 
			});			
	        Thread.Sleep(10);
	        map.unlock("a");
	        count.Wait();
			Assert.AreEqual(true, done);
			map.destroy();
	    }
		
		[Test()]
	    public void lockMap(){
	        HazelcastClient hClient = getHazelcastClient();
	        IMap<String, String> map = hClient.getMap<String, String>("lockMap");
	        CountdownEvent unlockLatch = new CountdownEvent(1);
	        CountdownEvent latch = new CountdownEvent(1);
	        map.put("a", "b");
	        map.lockMap(1000);
	        Assert.AreEqual(true, map.tryPut("a", "c", 10));
	        ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				Assert.AreEqual(false, map.lockMap(10));
                unlockLatch.Signal();
                Assert.AreEqual(true, map.lockMap(long.MaxValue));
                latch.Signal();
				map.unlockMap();
			});	
			unlockLatch.Wait(10000);
	        Assert.AreEqual(true, true);
	        Thread.Sleep(2000);
	        map.unlockMap();
			map.put("a", "d");
	        Assert.AreEqual("d", map.get("a"));
	        latch.Wait(10000);
			Assert.AreEqual(true, true);
			map.destroy();
	    }
		
		[Test()]
	    public void putToTheMap() {
	        HazelcastClient hClient = getHazelcastClient();
	        IMap<String, String> clientMap = hClient.getMap<String, String>("putToTheMap");
	        Assert.AreEqual(0, clientMap.size());
	        String result = clientMap.put("1", "CBDEF");
	        Assert.IsNull(result);
	        Assert.AreEqual("CBDEF", clientMap.get("1"));
	        Assert.AreEqual("CBDEF", clientMap.get("1"));
	        Assert.AreEqual("CBDEF", clientMap.get("1"));
	        Assert.AreEqual(1, clientMap.size());
	        result = clientMap.put("1", "B");
	        Assert.AreEqual("CBDEF", result);
	        Assert.AreEqual("B", clientMap.get("1"));
	        Assert.AreEqual("B", clientMap.get("1"));
			clientMap.destroy();
	    }
		
	[Test]
    public void putWithTTL() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("putWithTTL");
        Assert.AreEqual(0, map.size());
        map.put("1", "CBDEF", 100);
        Assert.AreEqual(1, map.size());
        Thread.Sleep(200);
        Assert.AreEqual(0, map.size());
			map.destroy();
    }
    
   

    [Test]
    public void tryPut() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("tryPut");
        Assert.AreEqual(0, map.size());
        bool result = map.tryPut("1", "CBDEF", 1000);
        Assert.IsTrue(result);
        Assert.AreEqual(1, map.size());
		map.destroy();
    }



    [Test]
    public void putAndGetEmployeeObjects() {
        HazelcastClient hClient = getHazelcastClient();
        int counter = 1000;
        IMap<String, Employee> clientMap = hClient.getMap<String, Employee>("putAndGetEmployeeObjects");
        for (int i = 0; i < counter; i++) {
            Employee employee = new Employee("name" + i, i, true, 5000 + i);
            employee.setMiddleName("middle" + i);
            employee.setFamilyName("familiy" + i);
            clientMap.put("" + i, employee);
        }
        for (int i = 0; i < counter; i++) {
            Employee e = clientMap.get("" + i);
            Assert.AreEqual("name" + i, e.getName());
            Assert.AreEqual("middle" + i, e.getMiddleName());
            Assert.AreEqual("familiy" + i, e.getFamilyName());
            Assert.AreEqual(i, e.getAge());
            Assert.AreEqual(true, e.isActive());
            Assert.AreEqual(5000 + i, e.getSalary(), 0);
        }
//        }
    }
    
		
		
    [Test]
    public void getPuttedValueFromTheMap() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> clientMap = hClient.getMap<String, String>("getPuttedValueFromTheMap");
        int size = clientMap.size();
        clientMap.put("1", "Z");
        String value = clientMap.get("1");
        Assert.AreEqual("Z", value);
        Assert.AreEqual(size + 1, clientMap.size());
		clientMap.remove("1");
			clientMap.destroy();
    }

    [Test]
    public void removeFromMap() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("removeFromMap");
        Assert.IsNull(map.put("a", "b"));
        Assert.AreEqual("b", map.get("a"));
        Assert.AreEqual("b", map.remove("a"));
        Assert.IsNull(map.remove("a"));
        Assert.IsNull(map.get("a"));
			map.destroy();
    }

    [Test]
    public void evictFromMap() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("evictFromMap");
        Assert.IsNull(map.put("a", "b"));
        Assert.AreEqual("b", map.get("a"));
        Assert.IsTrue(map.evict("a"));
        Assert.IsNull(map.get("a"));
			map.destroy();
    }
		
		
    public class Customer: DataSerializable {
        private String name;
        private int age;

        public Customer(String name, int age) {
            this.name = name;
            this.age = age;
        }

        public void setName(String name) {
            this.name = name;
        }

        public String getName() {
            return name;
        }

        public void setAge(int age) {
            this.age = age;
        }

        public int getAge() {
            return age;
        }
			
		public void writeData(IDataOutput dout){
			dout.writeInt(age);
			dout.writeUTF(name);
		}

   		public void readData(IDataInput din){
			this.age = din.readInt();
            this.name = din.readUTF();
		}	
			
		public String javaClassName(){
			return null;
		}     
    }

    [Test]
    public void getSize() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("getSize");
        Assert.AreEqual(0, map.size());
        map.put("a", "b");
        Assert.AreEqual(1, map.size());
        for (int i = 0; i < 100; i++) {
            map.put(""+i, ""+i);
        }
        Assert.AreEqual(101, map.size());
        map.remove("a");
        Assert.AreEqual(100, map.size());
        for (int i = 0; i < 50; i++) {
            map.remove(""+i);
        }
        Assert.AreEqual(50, map.size());
        for (int i = 50; i < 100; i++) {
            map.remove(""+i);
        }
        Assert.AreEqual(0, map.size());
			map.destroy();
    }
		/*
    [Test]
    public void valuesToArray() {
        HazelcastClient hClient = getHazelcastClient();
        IMap map = hClient.getMap("valuesToArray");
        Assert.AreEqual(0, map.size());
        map.put("a", "1");
        map.put("b", "2");
        map.put("c", "3");
        Assert.AreEqual(3, map.size());
        {
            Object[] values = map.values().toArray();
            Arrays.sort(values);
            assertArrayEquals(new Object[]{"1", "2", "3"}, values);
        }
        {
            String[] values = (String[]) map.values().toArray(new String[3]);
            Arrays.sort(values);
            assertArrayEquals(new String[]{"1", "2", "3"}, values);
        }
        {
            String[] values = (String[]) map.values().toArray(new String[2]);
            Arrays.sort(values);
            assertArrayEquals(new String[]{"1", "2", "3"}, values);
        }
        {
            String[] values = (String[]) map.values().toArray(new String[5]);
            Arrays.sort(values, 0, 3);
            assertArrayEquals(new String[]{"1", "2", "3", null, null}, values);
        }
    }
    */

    [Test]
    public void getMapEntry() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("getMapEntry");
        Assert.IsNull(map.put("a", "b"));
        map.get("a");
        map.get("a");
        MapEntry<String, String> entry = map.getMapEntry("a");
        Assert.AreEqual("a", entry.getKey());
        Assert.AreEqual("b", entry.getValue());
        Assert.AreEqual(2, entry.getHits());
        Assert.AreEqual("b", entry.getValue());
        Assert.AreEqual("b", entry.setValue("c"));
        Assert.AreEqual("c", map.get("a"));
        Assert.AreEqual("c", entry.getValue());
			
			map.destroy();
    }
		 
			 
    [Test]
    public void iterateOverMapKeys() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("iterateOverMapKeys");
        map.put("1", "A");
        map.put("2", "B");
        map.put("3", "C");
        System.Collections.Generic.ICollection<String> keySet = map.Keys();
        Assert.AreEqual(3, keySet.Count);
        foreach (String str in keySet) {
            map.remove(str);
        }
        Assert.AreEqual(0, map.size());
        
	}

    [Test]
    public void iterateOverMapEntries() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("iterateOverMapEntries");
        map.put("1", "A");
        map.put("2", "B");
        map.put("3", "C");
        IDictionary<String, String> entrySet = map.entrySet(null);
        Assert.AreEqual(3, entrySet.Count);
        System.Collections.Generic.ICollection<String> keySet = map.Keys();
        foreach (String key in entrySet.Keys) {
            Assert.IsTrue(keySet.Contains(key));
            Assert.AreEqual(entrySet[key], map.get(key));
        }
			
        foreach (String key in entrySet.Keys) {
            MapEntry<String, String> mapEntry = map.getMapEntry(key);
            Assert.AreEqual(1, mapEntry.getHits());
        }
		map.destroy();
    }
		 
    [Test]
    public void tryLock() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("tryLock");
        CountdownEvent latch = new CountdownEvent(3);
        map.put("1", "A");
        map.Lock("1");
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				if (!map.tryLock("1", 100)) {
                    latch.Signal();
                }
                if (!map.tryLock("1")) {
                    latch.Signal();
                }
                if (map.tryLock("2")) {
                    latch.Signal();
                }
					 
			});		
			Assert.IsTrue(latch.Wait(10000));
			map.destroy();
    }
		
	class CountDownLatchEntryListener<K, V> : EntryListener<K, V> {	
		CountdownEvent entryAddLatch;
	    CountdownEvent entryUpdatedLatch;
	    CountdownEvent entryRemovedLatch;
	
	    public CountDownLatchEntryListener(CountdownEvent entryAddLatch, CountdownEvent entryUpdatedLatch, CountdownEvent entryRemovedLatch) {
	        this.entryAddLatch = entryAddLatch;
	        this.entryUpdatedLatch = entryUpdatedLatch;
	        this.entryRemovedLatch = entryRemovedLatch;
	    }
			
	
	    public void entryAdded(EntryEvent<K, V> e) {
	        entryAddLatch.Signal();
	    }
	
	    public void entryRemoved(EntryEvent<K, V> e) {
	        entryRemovedLatch.Signal();
	    }
	
	    public void entryUpdated(EntryEvent<K, V> e) {
	        entryUpdatedLatch.Signal();
	    }
	
	    public void entryEvicted(EntryEvent<K, V> e) {
	        entryRemoved(e);
	    }
	}
	
	/*	
		
    [Test]
    public void addListener(){
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("addListener");
        map.clear();
        Assert.AreEqual(0, map.size());
        CountdownEvent entryAddLatch = new CountdownEvent(1);
        CountdownEvent entryUpdatedLatch = new CountdownEvent(1);
        CountdownEvent entryRemovedLatch = new CountdownEvent(1);
        CountDownLatchEntryListener<String, String> listener = new CountDownLatchEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        map.addEntryListener(listener, true);
        Assert.IsNull(map.get("hello"));
        map.put("hello", "world");
        map.put("hello", "new world");
        Assert.AreEqual("new world", map.get("hello"));
        map.remove("hello");
        Assert.IsTrue(entryAddLatch.Wait(10000));
        Assert.IsTrue(entryUpdatedLatch.Wait(10000));
        Assert.IsTrue(entryRemovedLatch.Wait(10000));
    }
		

    [Test]
    public void addListenerForKey(){
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("addListenerForKey");
        map.clear();
        Assert.AreEqual(0, map.size());
        CountdownEvent entryAddLatch = new CountdownEvent(1);
        CountdownEvent entryUpdatedLatch = new CountdownEvent(1);
        CountdownEvent entryRemovedLatch = new CountdownEvent(1);
        CountDownLatchEntryListener<String, String> listener = new CountDownLatchEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        map.addEntryListener(listener, "hello", true);
        Assert.IsNull(map.get("hello"));
        map.put("hello", "world");
        map.put("hello", "new world");
        Assert.AreEqual("new world", map.get("hello"));
        map.remove("hello");
        Assert.IsTrue(entryAddLatch.Wait(10000));
        Assert.IsTrue(entryUpdatedLatch.Wait(10000));
        Assert.IsTrue(entryRemovedLatch.Wait(10000));
    }

    [Test]
    public void addTwoListener1ToMapOtherToKey(){
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("addTwoListener1ToMapOtherToKey");
        CountdownEvent entryAddLatch = new CountdownEvent(5);
        CountdownEvent entryUpdatedLatch = new CountdownEvent(5);
        CountdownEvent entryRemovedLatch = new CountdownEvent(5);
        CountdownEventEntryListener<String, String> listener1 = new CountdownEventEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        CountdownEventEntryListener<String, String> listener2 = new CountdownEventEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        map.addEntryListener(listener1, true);
        map.addEntryListener(listener2, "hello", true);
        map.put("hello", "world");
        map.put("hello", "new world");
        map.remove("hello");
        Thread.Sleep(100);
        Assert.AreEqual(3, entryAddLatch.getCount());
        Assert.AreEqual(3, entryRemovedLatch.getCount());
        Assert.AreEqual(3, entryUpdatedLatch.getCount());
    }

    [Test]
    public void addSameListener1stToKeyThenToMap(){
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("addSameListener1stToKeyThenToMap");
        CountdownEvent entryAddLatch = new CountdownEvent(5);
        CountdownEvent entryUpdatedLatch = new CountdownEvent(5);
        CountdownEvent entryRemovedLatch = new CountdownEvent(5);
        CountdownEventEntryListener<String, String> listener1 = new CountdownEventEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        map.addEntryListener(listener1, "hello", true);
        map.addEntryListener(listener1, true);
        map.put("hello", "world");
        map.put("hello", "new world");
        map.remove("hello");
        Thread.Sleep(100);
        Assert.AreEqual(3, entryAddLatch.getCount());
        Assert.AreEqual(3, entryRemovedLatch.getCount());
        Assert.AreEqual(3, entryUpdatedLatch.getCount());
    }

    [Test]
    public void removeListener(){
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("removeListener");
        CountdownEvent entryAddLatch = new CountdownEvent(5);
        CountdownEvent entryUpdatedLatch = new CountdownEvent(5);
        CountdownEvent entryRemovedLatch = new CountdownEvent(5);
        CountdownEventEntryListener<String, String> listener1 = new CountdownEventEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        CountdownEventEntryListener<String, String> listener2 = new CountdownEventEntryListener<String, String>(entryAddLatch, entryUpdatedLatch, entryRemovedLatch);
        map.addEntryListener(listener1, true);
        map.put("hello", "world");
        map.put("hello", "new world");
        map.remove("hello");
        Thread.Sleep(100);
        Assert.AreEqual(4, entryAddLatch.getCount());
        Assert.AreEqual(4, entryRemovedLatch.getCount());
        Assert.AreEqual(4, entryUpdatedLatch.getCount());
        map.removeEntryListener(listener1);
        map.put("hello", "world");
        map.put("hello", "new world");
        map.remove("hello");
        Thread.Sleep(100);
        Assert.AreEqual(4, entryAddLatch.getCount());
        Assert.AreEqual(4, entryRemovedLatch.getCount());
        Assert.AreEqual(4, entryUpdatedLatch.getCount());
    }
		 
		 */
		
		
    [Test]
    public void putIfAbsent() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("putIfAbsent");
        String result = map.put("1", "CBDEF");
        Assert.IsNull(result);
        Assert.IsNull(map.putIfAbsent("2", "C"));
        Assert.AreEqual("C", map.putIfAbsent("2", "D"));
		map.destroy();
    }

    [Test]
    public void putIfAbsentWithTtl() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("putIfAbsentWithTtl");
        String result = map.put("1", "CBDEF");
        Assert.IsNull(result);
        Assert.IsNull(map.putIfAbsent("2", "C", 50));
        Assert.AreEqual(2, map.size());
        Assert.AreEqual("C", map.putIfAbsent("2", "D", 50));
        Thread.Sleep(100);
        Assert.AreEqual(1, map.size());
		map.destroy();
    }
		

    [Test]
    public void removeIfSame() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("remove");
        String result = map.put("1", "CBDEF");
        Assert.IsNull(result);
        Assert.IsFalse(map.remove("1", "CBD"));
        Assert.AreEqual("CBDEF", map.get("1"));
        Assert.IsTrue(map.remove("1", "CBDEF"));
			map.destroy();
    }

    [Test]
    public void replace() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<String, String> map = hClient.getMap<String, String>("replace");
        String result = map.put("1", "CBDEF");
        Assert.IsNull(result);
        Assert.AreEqual("CBDEF", map.replace("1", "CBD"));
        Assert.IsNull(map.replace("2", "CBD"));
        Assert.IsFalse(map.replace("2", "CBD", "ABC"));
        Assert.IsTrue(map.replace("1", "CBD", "XX"));
		map.remove("1");
		map.remove("2");
			map.destroy();
    }
	

    [Test]
    public void clear() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?, int?> map = hClient.getMap<int?, int?>("clear");
        for (int i = 0; i < 100; i++) {
            Assert.IsNull(map.put(i, i));
            Assert.AreEqual(i, map.get(i));
        }
        map.clear();
        for (int i = 0; i < 100; i++) {
            Assert.IsNull(map.get(i));
        }
			map.destroy();
    }

    [Test]
    public void destroyMap() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?, int?> map = hClient.getMap<int?, int?>("destroy");
        for (int i = 0; i < 100; i++) {
            Assert.IsNull(map.put(i, i));
            Assert.AreEqual(i, map.get(i));
        }
        IMap<int?, int?> map2 = hClient.getMap<int?, int?>("destroy");
        Assert.IsTrue(map == map2);
        Assert.IsTrue(map.getId().Equals(map2.getId()));
        map.destroy();

        for (int i = 0; i < 100; i++) {
            Assert.IsNull(map2.get(i));
        }
			map.destroy();
    }
		
	[Test]
    public void returnNunNullableObjectWithNull() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int, int> map = hClient.getMap<int, int>("destroy");
       	Assert.AreEqual(0, map.get(1));
		Assert.AreEqual(0, map.put(1, 1));
       map.destroy();
    }
		
	
		
    [Test]
    public void containsKey() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?,int?> map = hClient.getMap<int?,int?>("containsKey");
        int counter = 100;
        for (int i = 0; i < counter; i++) {
            Assert.IsNull(map.put(i, i));
            Assert.AreEqual(i, map.get(i));
        }
        for (int i = 0; i < counter; i++) {
            Assert.IsTrue(map.containsKey(i));
        }
			map.destroy();
    }

    [Test]
    public void containsValue() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?,int?> map = hClient.getMap<int?,int?>("containsValue");
        int counter = 100;
        for (int i = 0; i < counter; i++) {
            Assert.IsNull(map.put(i, i));
            Assert.AreEqual(i, map.get(i));
        }
        for (int i = 0; i < counter; i++) {
            Assert.IsTrue(map.containsValue(i));
        }
			map.destroy();
    }
		
		
		
    [Test]
    public void isEmpty() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?,int?> map = hClient.getMap<int?,int?>("isEmpty");
        int counter = 100;
        Assert.IsTrue(map.size()==0);
        for (int i = 0; i < counter; i++) {
            Assert.IsNull(map.put(i, i));
            Assert.AreEqual(i, map.get(i));
        }
        Assert.IsFalse(map.size()==0);
			map.destroy();
    }

    [Test]
    public void putAll() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?,int?> map = hClient.getMap<int?,int?>("putAll");
        int counter = 100;
        HashSet<int?> keys = new HashSet<int?>();
        for (int i = 0; i < counter; i++) {
            keys.Add(i);
        }
        Dictionary<int?, int?> all = map.getAll(keys);
        Assert.AreEqual(0, all.Count);
        Dictionary<int?, int?> tempMap = new Dictionary<int?, int?>();
        for (int i = 0; i < counter; i++) {
            tempMap.Add(i, i);
        }
        map.putAll(tempMap);
        for (int i = 0; i < counter; i++) {
            Assert.AreEqual(i, map.get(i));
        }
        all = map.getAll(keys);
        Assert.AreEqual(counter, all.Count);
		map.destroy();
    }

    [Test]
    public void putAllMany() {
        HazelcastClient hClient = getHazelcastClient();
        IMap<int?,int?> map = hClient.getMap<int?,int?>("putAllMany");
        int counter = 100;
        for (int j = 0; j < 4; j++, counter *= 10) {
            Dictionary<int?, int?> tempMap = new Dictionary<int?, int?>();
            for (int i = 0; i < counter; i++) {
                tempMap.Add(i, i);
            }
            map.putAll(tempMap);
            Assert.AreEqual(1, map.get(1));
        }
        map.destroy();
    }

		


 
    

	    [Test]
	    public void testGetNullMapEntry() {
	        HazelcastClient hClient = getHazelcastClient();
	        IMap<String, String> imap = hClient.getMap<String, String>("testGetNullMapEntry");
	        String key = "key";
	        MapEntry<String, String> mapEntry = imap.getMapEntry(key);
	        Assert.IsNull(mapEntry);
	    }
	
	    [Test]
	    [Ignore]
		public void testSqlPredicate() {
			//IOUtil.printBytes(IOUtil.toByte(new Employee("" + 1, 1, 1 % 2 == 0, 1)));
			
	        HazelcastClient hClient = getHazelcastClient();
	        IMap<int?, Employee> map = hClient.getMap<int?, Employee>("testSqlPredicate");
	        for (int i = 0; i < 100; i++) {
	            map.put(i, new Employee("" + i, i, i % 2 == 0, i));
	        }
	        System.Collections.Generic.ICollection<Employee> set = map.Values(new SqlPredicate("active AND age < 30"));
	        foreach (Employee e in set) {
	            Assert.IsTrue(e.getAge() < 30);
	            Assert.IsTrue(e.isActive());
	        }
	    }

		[Test]
		public void serializeEmployee(){
			HazelcastClient hClient = getHazelcastClient();
			IMap<String, Employee> clientMap = hClient.getMap<String, Employee>("employee");
			Employee employee = new Employee("name", 34, true, 1);
			clientMap.put("1", employee);
			Console.WriteLine("PUT");
			Assert.IsTrue(34 == clientMap.get("1").getAge());
			Assert.IsTrue("name".Equals(clientMap.get("1").getName()));
			Assert.IsTrue(true == clientMap.get("1").isActive());
			//Assert.IsTrue(1 == clientMap.get("1").getSalary());
		}

            
        [Test]
        public void testMapPutAndGetShort() {
            HazelcastClient hClient = getHazelcastClient();
            IMap<String, short> map = hClient.getMap<String, short>("testMapPutAndGet2");

            short value1 = 1;
            short value = map.put("key", value1);

            Assert.AreEqual(value1, map.get("key"));
            Assert.AreEqual(1, map.size());
            //Assert.IsNull(value);

            value = map.put("key", value1);
            Assert.AreEqual(value1, map.get("key"));
            Assert.AreEqual(1, map.size());
            Assert.AreEqual(value1, value);

            short newValue = 2;
            value = map.put("key", newValue);
            Assert.AreEqual(value1, value);
            Assert.AreEqual(newValue, map.get("key"));
            Assert.AreEqual(1, map.size());

            //var entry = map.getMapEntry("key");
            //Assert.AreEqual("Hello", entry.getKey());
            //Assert.AreEqual(newValue, entry.getValue());
        }

	    [Test]
	    public void testMapPutAndGet()
	    {
            testMapPutAndGetGeneric<short>(1,2);
            testMapPutAndGetGeneric<byte>(1, 2);
            testMapPutAndGetGeneric<sbyte>(1, 2);
            testMapPutAndGetGeneric<uint>(1, 2);
            testMapPutAndGetGeneric<ulong>(1, 2);
            testMapPutAndGetGeneric<ushort>(1, 2);
            testMapPutAndGetGeneric<string>("1", "2");
            testMapPutAndGetGeneric<int>(1, 2);
	    }
        
        private void testMapPutAndGetGeneric<T>(T value1,T newValue) {
            HazelcastClient hClient = getHazelcastClient();
            IMap<String, T> map = hClient.getMap<String, T>("testMapPutAndGetGeneric"+typeof(T).Name);

            T value = map.put("key", value1);

            Assert.AreEqual(value1, map.get("key"));
            Assert.AreEqual(1, map.size());
            //Assert.IsNull(value);

            value = map.put("key", value1);
            Assert.AreEqual(value1, map.get("key"));
            Assert.AreEqual(1, map.size());
            Assert.AreEqual(value1, value);

            value = map.put("key", newValue);
            Assert.AreEqual(value1, value);
            Assert.AreEqual(newValue, map.get("key"));
            Assert.AreEqual(1, map.size());

        }

	}
	
	class Employee: DataSerializable {
        String name;
        String familyName;
        String middleName;
        long age;
        bool active;
        double salary;
		
		static String className = "com.hazelcast.client.HazelcastClientMapTest$Employee";
		
        public Employee() {
        }

        public Employee(String name, long age, bool live, double price) {
            this.name = name;
            this.age = age;
            this.active = live;
            this.salary = price;
        }

        public String getMiddleName() {
            return middleName;
        }

        public void setMiddleName(String middleName) {
            this.middleName = middleName;
        }

        public String getFamilyName() {
            return familyName;
        }

        public void setFamilyName(String familyName) {
            this.familyName = familyName;
        }

        public String getName() {
            return name;
        }

        public long getAge() {
            return age;
        }

        public double getSalary() {
            return salary;
        }

        public bool isActive() {
            return active;
        }

        public void writeData(Hazelcast.IO.IDataOutput dout) {
            dout.writeBoolean(name!=null);
            if(name!=null)
                dout.writeUTF(name);
            dout.writeBoolean(familyName!=null);
            if(familyName!=null)
                dout.writeUTF(familyName);
            dout.writeBoolean(middleName!=null);
            if(middleName!=null)
                dout.writeUTF(middleName);
            dout.writeLong(age);
            dout.writeBoolean(active);
            dout.writeDouble(salary);

        }

        public void readData(Hazelcast.IO.IDataInput din){
            if(din.readBoolean())
                this.name = din.readUTF();
            if(din.readBoolean())
                this.familyName = din.readUTF();
            if(din.readBoolean())
                this.middleName = din.readUTF();

            this.age = din.readLong();
            this.active = din.readBoolean();
            this.salary = din.readDouble();
        }
			
		public String javaClassName(){
			return Employee.className;;
		}
		
	}
}