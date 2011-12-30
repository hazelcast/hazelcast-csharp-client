using System;
using System.Collections;
using System.Threading;
using System.Collections.Concurrent;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class Call
	{
		readonly long id;
		readonly Packet request;
		volatile Packet response;
		long sent = 0;
		long written = 0;
		long received = 0;
		long replied = 0;
		volatile bool fireNforget = false;
		
		static long callIdGen = 0;
		
		readonly BlockingCollection<Packet> inbQ = new BlockingCollection<Packet>(1);
		
		public Call (Packet request)
		{
			this.id = incrementCallId();
			this.request = request;
			this.request.callId = id;
		}

		public long getId ()
		{
			return id;
		}
		
		public bool FireNforget {
			get {
				return this.fireNforget;
			}
			set {
				fireNforget = value;
			}
		}
		

		public Packet getResult ()
		{
			//Packet response = inQ.Dequeue ();
			Packet response = inbQ.Take();
			return response;
		}

		public void setResult (Packet response)
		{
			//this.inQ.Enqueue (response);
			inbQ.Add(response);
		}
		public Packet getRequest ()
		{
			return request;
		}

		private static long incrementCallId ()
		{
			long initialValue, computedValue;
			do {
				initialValue = callIdGen;
				computedValue = initialValue + 1;
				
			} while (initialValue != Interlocked.CompareExchange (ref callIdGen, computedValue, initialValue));
			return computedValue;
		}
		
		public void onDisconnect(Member member, OutThread outThread) {
        	if (!outThread.contains(this)) {
            	//logger.log(Level.FINEST, "Re enqueue " + this);
            	outThread.enQueue(this);
           	}
      	}
	}
}

