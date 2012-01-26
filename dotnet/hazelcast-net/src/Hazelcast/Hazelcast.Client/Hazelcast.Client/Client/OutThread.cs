using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Hazelcast.Client
{
	public class OutThread:ClientThread 	
	{
		TcpClient tcpClient;
		ConcurrentDictionary<long, Call> calls;

		//BlockingQueue<Call> inQ = new BlockingQueue<Call> (1000);
		BlockingCollection<Call> inQ = new BlockingCollection<Call>(1000);
		public OutThread (TcpClient tcpClient, ConcurrentDictionary<long, Call> calls)
		{
			this.tcpClient = tcpClient;
			this.calls = calls;
		}

		protected override void customRun ()
		{
			Call call = (Call)inQ.Take();
			if(!call.FireNforget)
				calls.AddOrUpdate (call.getId (), call, null);
			Packet packet = call.getRequest ();
			if (packet != null) {
				send (packet);
			}
		}
		
		public bool contains(Call call){
			return inQ.Contains(call);
		}
		
		public void send (Packet packet)
		{
			NetworkStream stream = tcpClient.GetStream();
			packet.write (stream);
			//tcpClient.GetStream ().Flush ();
		}


		public void enQueue (Call call)
		{
			inQ.Add(call);
		}

		public static OutThread start (TcpClient tcpClient, ConcurrentDictionary<long, Call> calls, String prefix)
		{
			OutThread outThread = new OutThread (tcpClient, calls);
			Thread thread = new Thread (new ThreadStart (outThread.run));
			thread.Name = prefix + "OutThread";
			thread.Start ();
			return outThread;
		}
		
		public void shutdown(){
			
		}
	}
}

