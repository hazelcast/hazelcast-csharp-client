using System;
using System.Collections;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class Call
	{
		readonly long id;
		readonly Packet request;
		
		//public  Stopwatch pre = new Stopwatch();
		//public  Stopwatch on = new Stopwatch();
		//public  Stopwatch post = new Stopwatch();
		volatile bool fireNforget = false;
		
		public System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
	
		
		
		static long callIdGen = 0;
		
		readonly BlockingCollection<Object> inbQ = new BlockingCollection<Object>(1);
		
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
		

		public Packet getResult (int timeout)
		{
			Object response = null; 
            inbQ.TryTake(out response, timeout);
            if (response is Exception) {
                throw (Exception)response;
            }
            else 
			    return (Packet) response;
		}

		public void setResult (Object response)
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
		/*
		private static long preClient = 0;
		
		private static long onServer = 0;
		
    	private static long postClient = 0;

	    public void collect() {
	        //Console.WriteLine(pre.ElapsedTicks / (Stopwatch.Frequency / 1000));
	        //Console.WriteLine(on.ElapsedTicks / (Stopwatch.Frequency / 1000));
	        //Console.WriteLine(post.ElapsedTicks / (Stopwatch.Frequency / 1000));
			Interlocked.Add(ref preClient, pre.ElapsedTicks);
			Interlocked.Add(ref onServer, on.ElapsedTicks);
			Interlocked.Add(ref postClient, post.ElapsedTicks);
	    }
	    public static void printAndReset(){
	        long pre = Interlocked.Exchange(ref preClient, 0);
	        long on = Interlocked.Exchange(ref onServer, 0);
	        long post = Interlocked.Exchange(ref postClient, 0);
			Console.WriteLine("Total pre  in ms>     " + pre/(Stopwatch.Frequency/1000));
	        Console.WriteLine("Total onServer  in ms>" + on / (Stopwatch.Frequency / 1000));
	        Console.WriteLine("Total After in ms >   " + post / (Stopwatch.Frequency / 1000));
	    }*/
	}
}

