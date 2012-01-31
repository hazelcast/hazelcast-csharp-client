using System;
using NUnit.Framework;
using Hazelcast.Core;
using System.Threading;

namespace Hazelcast.Client.Tests
{
	[TestFixture()]
	public class ClusterTest: HazelcastTest, InstanceListener
	{
		DateTime dt = new DateTime(1970,1,1);
		
		[Test()]
		public void clusterTime ()
		{
			HazelcastClient client = getHazelcastClient();
			ICluster cluster = client.getCluster();
			long clusterTime = cluster.getClusterTime();
			dt.AddMilliseconds(clusterTime);
			Console.WriteLine("ClusterTime: " + clusterTime);
		}
		
		[Test()]
		public void members()
		{
			HazelcastClient client = getHazelcastClient();
			ICluster cluster = client.getCluster();
			System.Collections.Generic.ICollection<Member> collection = cluster.getMembers();
			Console.WriteLine("size is " + collection.Count);
			foreach(Member m in collection){
				Console.WriteLine(m);
			}
		}
		
		[Test()]
		public void instances()
		{
			HazelcastClient client = getHazelcastClient();			
			client.getMap<String, String>("mmap").put("a", "b");
			client.getQueue<String>("qqqq").offer("a");
			client.getTopic<String>("tttopic").publish("m");
			client.getLock("lock").Lock();
			client.getLock("lock").unLock();
			System.Collections.Generic.ICollection<Instance> collection = client.getInstances();
			Console.WriteLine("size is " + collection.Count);
			foreach(Instance instance in collection){
				Console.WriteLine(instance.getInstanceType() + ": " + instance.getId());
			}
		}
		
		
		
		[Test()]
		[Ignore]
		public void membershipListener()
		{
			HazelcastClient client = getHazelcastClient();
			ICluster cluster = client.getCluster();
			
			CountdownEvent added = new CountdownEvent(1);
			CountdownEvent removed = new CountdownEvent(1);
			cluster.addMembershipListener(new MyMembershipListener(added, removed));
			
			foreach(Member m in cluster.getMembers()){
				Console.WriteLine(m);
			}
			
			Assert.IsTrue(added.Wait(20000));
			Assert.IsTrue(removed.Wait(20000));
			
		}
		
		class MyMembershipListener: MembershipListener{
			CountdownEvent added;
			CountdownEvent removed;
				
			public MyMembershipListener (CountdownEvent added, CountdownEvent removed){
				this.added = added;
				this.removed = removed;
			}
			public void memberAdded(MembershipEvent membershipEvent){
				added.Signal();
				Console.WriteLine(membershipEvent);
			}
	
		    public void memberRemoved(MembershipEvent membershipEvent){
				removed.Signal();	
				Console.WriteLine(membershipEvent);
			}
		}
		
		[Test()]
		[Ignore]
		public void instanceListener()
		{
			HazelcastClient client = getHazelcastClient();		
			
			CountdownEvent added = new CountdownEvent(1);
			CountdownEvent removed = new CountdownEvent(1);
			client.addInstanceListener(new MyInstanceListener(added, removed));
			//client.addInstanceListener(this);
			IMap<String, String> ins = client.getMap<String, String>("someNewMap");
			ins.put("1","1");
			ins.destroy();
			Assert.IsTrue(added.Wait(5000));
			Assert.IsTrue(removed.Wait(5000));
			
		}
		
		class MyInstanceListener: InstanceListener{
			CountdownEvent added;
			CountdownEvent removed;
				
			public MyInstanceListener (CountdownEvent added, CountdownEvent removed){
				this.added = added;
				this.removed = removed;
			}
			public void instanceCreated(InstanceEvent e){
				added.Signal();
				Console.WriteLine(e);
			}
	
		    public void instanceDestroyed(InstanceEvent e){
				removed.Signal();	
				Console.WriteLine(e);
			}
		}
		public void instanceCreated(InstanceEvent e){
			Console.WriteLine(e);
		}
	
	
	    public void instanceDestroyed(InstanceEvent e){
			Console.WriteLine(e);
		}
	}
}

