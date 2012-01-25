using System;
using System.Threading;
using NUnit.Framework;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class LockTest: HazelcastTest
	{
	[Test]
	[ExpectedException(typeof(NullReferenceException)) ]	
    public void testLockNull() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ILock _lock = hClient.getLock(null);
        _lock.Lock();
    }

    [Test]
    public void testLockUnlock() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ILock _lock = hClient.getLock("testLockUnlock");
        _lock.Lock();
        CountdownEvent latch = new CountdownEvent(1);
        CountdownEvent unlockLatch = new CountdownEvent(1);
			
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				Assert.IsFalse(_lock.tryLock());
                unlockLatch.Signal();
                _lock.Lock();
                latch.Signal();
			});
			
       
        Assert.IsTrue(unlockLatch.Wait(10000));
        _lock.unLock();
        Assert.IsTrue(latch.Wait(10));
    }

    [Test]
    public void testTryLock() {
        HazelcastClient hClient = getHazelcastClient();
        Hazelcast.Core.ILock _lock = hClient.getLock("testTryLock");
        Assert.IsTrue(_lock.tryLock());
        _lock.Lock();
        CountdownEvent latch = new CountdownEvent(1);
        CountdownEvent unlockLatch = new CountdownEvent(1);
        
			ThreadPool.QueueUserWorkItem(
				(obj) => 
			{
				Assert.IsFalse(_lock.tryLock());
                unlockLatch.Signal();
               	Assert.IsTrue(_lock.tryLock(10000));
                latch.Signal();
			});
		
        
        Assert.IsTrue(unlockLatch.Wait(10000));
        _lock.unLock();
        _lock.unLock();
        Assert.IsTrue(latch.Wait(10000));
    }
	}
}

