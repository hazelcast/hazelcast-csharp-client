using System;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class AtomicNumberTest :HazelcastTest
	{
		[Test()]
	    public void testAtomicLong() {
	        HazelcastClient client = getHazelcastClient();
	        IAtomicNumber an = client.getAtomicNumber("testAtomicLong");
	        Assert.AreEqual(0, an.get());
	        Assert.AreEqual(-1, an.decrementAndGet());
	        Assert.AreEqual(0, an.incrementAndGet());
	        Assert.AreEqual(1, an.incrementAndGet());
	        Assert.AreEqual(2, an.incrementAndGet());
	        Assert.AreEqual(1, an.decrementAndGet());
	        Assert.AreEqual(1, an.getAndSet(23));
	        Assert.AreEqual(28, an.addAndGet(5));
	        Assert.AreEqual(28, an.get());
	        Assert.AreEqual(28, an.getAndAdd(-3));
	        Assert.AreEqual(24, an.decrementAndGet());
	        Assert.IsFalse(an.compareAndSet(23, 50));
	        Assert.IsTrue(an.compareAndSet(24, 50));
	        Assert.IsTrue(an.compareAndSet(50, 0));
	    }
	
	    [Test]
	    public void testSimple() {
	        HazelcastClient client = getHazelcastClient();
	
	        String name = "simple";
	        
	        IAtomicNumber clientAtomicLong = client.getAtomicNumber(name);
	        
			Assert.AreEqual(0L, clientAtomicLong.get());
	
	        Assert.AreEqual(1L, clientAtomicLong.incrementAndGet());
	        Assert.AreEqual(1L, clientAtomicLong.get());
	
	        Assert.AreEqual(1L, clientAtomicLong.getAndAdd(1));
	        Assert.AreEqual(2L, clientAtomicLong.get());
	
	        Assert.AreEqual(3L, clientAtomicLong.addAndGet(1L));
	        Assert.AreEqual(3L, clientAtomicLong.get());
	
	        clientAtomicLong.set(3L);
	        Assert.AreEqual(3L, clientAtomicLong.get());
	
	        Assert.IsFalse(clientAtomicLong.compareAndSet(4L, 1L));
	        Assert.AreEqual(3L, clientAtomicLong.get());
	
	        Assert.IsTrue(clientAtomicLong.compareAndSet(3L, 1L));
	        Assert.AreEqual(1L, clientAtomicLong.get());
	    }
	}
}

