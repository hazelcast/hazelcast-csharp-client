using System;
using NUnit.Framework;
using System.Threading;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class CountDownLatchTest: HazelcastTest
	{
		
		/*
		[Test]
	    public void testClientCountDownLatchSimple() {
	        HazelcastClient client1 = newHazelcastClient();
	        HazelcastClient client2 = newHazelcastClient();
			CountdownEvent e = new CountdownEvent();
			e.
	        ICountDownLatch cdl1 = client1.getCountDownLatch("test");
	        ICountDownLatch cdl2 = client2.getCountDownLatch("test");
	        Member c1Member = clientToMember(client1); 
	        AtomicInteger result = new AtomicInteger();
	        int count = 5;
	        cdl1.setCount(count);
	        Assert.AreEqual(c1Member, ((CountDownLatchClientProxy) cdl2).getOwner());
	        ThreadPool.QueueUserWorkItem(
				(obj) => 
				{
					if(cdl2.await(1000, TimeUnit.MILLISECONDS))
		            result.incrementAndGet();
				});
			
	        for (int i = count; i > 0; i--){
	            Assert.AreEqual(i, ((CountDownLatchClientProxy) cdl2).getCount());
	            cdl1.countDown();
	            Thread.Sleep(100);
	        }
	        Assert.AreEqual(1, result.get());
	    }
	
	    [Test]
	    public void testClientCountDownLatchOwnerLeft() {
	        HazelcastInstance instance = Hazelcast.newHazelcastInstance(null);
	        HazelcastClient client1 = newHazelcastClient(instance);
	        HazelcastClient client2 = newHazelcastClient(instance);
	        ICountDownLatch cdl1 = client1.getCountDownLatch("test");
	        ICountDownLatch cdl2 = client2.getCountDownLatch("test");
	        Member c1Member = clientToMember(client1); 
	        AtomicInteger result = new AtomicInteger();
	        cdl1.setCount(1);
	        Assert.AreEqual(1, ((CountDownLatchClientProxy) cdl2).getCount());
	        Assert.AreEqual(c1Member, ((CountDownLatchClientProxy) cdl2).getOwner());
	        ThreadPool.QueueUserWorkItem(
				(obj) => 
				{
					try {
	                    // should throw MemberLeftException
	                    cdl2.await(1000, TimeUnit.MILLISECONDS);
	                    fail();
	                } catch (MemberLeftException e) {
	                    result.incrementAndGet();
	                } catch (Throwable e) {
	                    e.printStackTrace();
	                    fail();
	                }
				});
		
	        Thread.Sleep(200);
	        client1.shutdown();
	        thread.join();
	        Assert.AreEqual(1, result.get());
	    }
	
	    [Test]
	    public void testClientCountDownLatchInstanceDestroyed() {
	        HazelcastInstance instance = Hazelcast.newHazelcastInstance(null);
	        HazelcastClient client1 = newHazelcastClient(instance);
	        HazelcastClient client2 = newHazelcastClient(instance);
	        ICountDownLatch cdl1 = client1.getCountDownLatch("test");
	        ICountDownLatch cdl2 = client2.getCountDownLatch("test");
	        Member c1Member = clientToMember(client1); 
	        AtomicInteger result = new AtomicInteger();
	        cdl1.setCount(1);
	        Assert.AreEqual(1, ((CountDownLatchClientProxy) cdl2).getCount());
	        Assert.AreEqual(c1Member, ((CountDownLatchClientProxy) cdl2).getOwner());
			ThreadPool.QueueUserWorkItem(
				(obj) => 
				{
					 try {
	                    // should throw InstanceDestroyedException
	                    cdl2.await(1000, TimeUnit.MILLISECONDS);
	                    fail();
	                } catch (InstanceDestroyedException e) {
	                    result.incrementAndGet();
	                } catch (Throwable e) {
	                    e.printStackTrace();
	                    fail();
	                }
				});
		

			
			
	        
	        Thread.Sleep(200);
	        cdl1.destroy();
	        thread.join();
	        Assert.AreEqual(1, result.get());
	    }
	
	    [Test]
	    public void testClientCountDownLatchClientShutdown() {
	        HazelcastInstance instance = Hazelcast.newHazelcastInstance(null);
	        HazelcastClient client1 = newHazelcastClient(instance);
	        HazelcastClient client2 = newHazelcastClient(instance);
	        ICountDownLatch cdl1 = client1.getCountDownLatch("test");
	        ICountDownLatch cdl2 = client2.getCountDownLatch("test");
	        Member c1Member = clientToMember(client1); 
	        AtomicInteger result = new AtomicInteger();
	        cdl1.setCount(1);
	        Assert.AreEqual(1, ((CountDownLatchClientProxy) cdl2).getCount());
	        Assert.AreEqual(c1Member, ((CountDownLatchClientProxy) cdl2).getOwner());
			ThreadPool.QueueUserWorkItem(
				(obj) => 
				{
					try {
	                    // should throw IllegalStateException
	                    cdl1.await(1000, TimeUnit.MILLISECONDS);
	                } catch (IllegalStateException e) {
	                    result.incrementAndGet();
	                    return;
	                } catch (Throwable e) {
	                    e.printStackTrace();
	                }
	                fail();
				});
	        Thread.Sleep(20);
	        client1.shutdown();
	        thread.join();
	        Assert.AreEqual(1, result.get());
	    }
	    */
	}
}

