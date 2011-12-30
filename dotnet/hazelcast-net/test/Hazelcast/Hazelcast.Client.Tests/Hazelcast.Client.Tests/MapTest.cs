using System;
using NUnit.Framework;
using Hazelcast.Client;
using System.Threading;

namespace Hazelcast.Client.Tests
{
	//[TestFixture()]
	public class MapTest
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
			});	
			unlockLatch.Wait(10000);
	        Assert.AreEqual(true, true);
	        Thread.Sleep(2000);
	        map.unlockMap();
	        Assert.AreEqual("c", map.get("a"));
	        latch.Wait(10000);
			Assert.AreEqual(true, true);
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
	    }
		
	
		
		
		public static HazelcastClient getHazelcastClient(){
			return HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");	
		}
	}
}

