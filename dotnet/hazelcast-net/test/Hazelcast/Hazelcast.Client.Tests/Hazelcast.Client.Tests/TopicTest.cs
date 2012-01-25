using System;
using System.Threading;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class TopicTest: HazelcastTest
	{
		[Test]
		[ExpectedException(typeof(NullReferenceException)) ]
	    public void testAddNull(){
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("testAddNull");
	        topic.publish(null);
	    }
	
	    [Test]
	    public void testName() {
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("testName");
	        Assert.AreEqual("testName", topic.getName());
	    }
		
		class MyMessageListener<E>: MessageListener<E>{
			CountdownEvent latch;
			E expected;
			public MyMessageListener(CountdownEvent latch, E message){
				this.latch = latch;
				expected = message;
			}
			
			public void onMessage<E>(Message<E> msg) {
	         	Console.WriteLine("Received: " + msg.getMessageObject());
				if ("".Equals(expected) || msg.getMessageObject().Equals(expected)) {
	                    latch.Signal();
	           	}
	       	}
			
		}
	
	    [Test]
	    public void addMessageListener(){
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("addMessageListener");
	        CountdownEvent latch = new CountdownEvent(1);
	        String message = "Hazelcast Rocks!";
	        topic.addMessageListener(new MyMessageListener<String>(latch, message) );
	        topic.publish(message);
	        Assert.IsTrue(latch.Wait(10000));
	    }
	
	    [Test]
	    public void addTwoMessageListener(){
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("addTwoMessageListener");
	        CountdownEvent latch = new CountdownEvent(2);
	        String message = "Hazelcast Rocks!";
	        topic.addMessageListener(new MyMessageListener<String>(latch, message) );
	  		topic.addMessageListener(new MyMessageListener<String>(latch, message) );
			
	        topic.publish(message);
	        Assert.IsTrue(latch.Wait(10000));
	    }
	
	    [Test]
		[Ignore]
	    public void removeMessageListener(){
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("removeMessageListener");
	        CountdownEvent latch = new CountdownEvent(2);
			
			MyMessageListener<String> messageListener = new MyMessageListener<String>(latch, "");
			topic.addMessageListener(messageListener);
	        topic.publish("message1");
	        Thread.Sleep(100);
			Assert.AreEqual(1, latch.CurrentCount);
	        topic.removeMessageListener(messageListener);
	        topic.publish("message2");
	        Thread.Sleep(100);
	        Assert.AreEqual(1, latch.CurrentCount);
	    }
	
	    [Test]
	    public void testTenTimesRemoveMessageListener(){
	        CountdownEvent latch = new CountdownEvent(10);
		    ThreadPool.QueueUserWorkItem(
					(obj) => 
				{
					for (int i = 0; i < 10; i++) {
		             	removeMessageListener();
		                latch.Signal();
		           	}
				});    
			
		        Assert.IsTrue(latch.Wait(20000));
		    }
	
	    [Test]
	    public void testPerformance(){
	        HazelcastClient hClient = getHazelcastClient();
	        System.DateTime begin = System.DateTime.Now;
	        int count = 10000;
	        ITopic<String> topic = hClient.getTopic<String>("perf");
	        CountdownEvent l = new CountdownEvent(count);
	        for (int i = 0; i < count; i++) {
	        	ThreadPool.QueueUserWorkItem(
					(obj) => 
				{
					topic.publish("my object");
	                l.Signal();
				});    
	        }
	        Assert.IsTrue(l.Wait(20000));
	        double time = (System.DateTime.Now - begin).TotalMilliseconds;
	        Console.WriteLine("per second: " + count * 1000 / time);
	    }
	
	    [Test]
	    public void add2listenerAndRemoveOne(){
	        HazelcastClient hClient = getHazelcastClient();
	        ITopic<String> topic = hClient.getTopic<String>("removeMessageListener");
	        CountdownEvent latch = new CountdownEvent(4);
	        String message = "Hazelcast Rocks!";
			
			MyMessageListener<String> messageListener1 = new MyMessageListener<String>(latch, "");
			MyMessageListener<String> messageListener2 = new MyMessageListener<String>(latch, "");
			
			topic.addMessageListener(messageListener1);
			topic.addMessageListener(messageListener2);
			
	        topic.publish(message + "1");
	        Thread.Sleep(100);
	        topic.removeMessageListener(messageListener1);
	        
	        topic.publish(message + "2");
	        Thread.Sleep(100);
	        Assert.AreEqual(1, latch.CurrentCount);
	    }
	}
}

