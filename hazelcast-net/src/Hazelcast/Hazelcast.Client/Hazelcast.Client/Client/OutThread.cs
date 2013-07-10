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
		readonly ConnectionManager connectionManager;
		Connection connection;
			
		readonly ConcurrentDictionary<long, Call> calls;
		
		readonly BlockingCollection<Call> inQ = new BlockingCollection<Call>(1000);
		public OutThread (ConnectionManager connectionManager, ConcurrentDictionary<long, Call> calls)
		{
			this.connectionManager = connectionManager;
			this.calls = calls;
		}

		protected override void customRun ()
		{
			Call call = inQ.Take();
			//call.pre.Stop();
			//call.on.Start();
			if(!call.FireNforget)
				calls.TryAdd(call.getId (), call);

			Packet packet = call.getRequest ();
			if (packet != null) {
				connection = connectionManager.getConnection();
				if(connection == null){
					throw new Exception("No Connection");
				}
				write(connection, packet);
			}
		}
		
		public bool contains(Call call){
			return inQ.Contains(call);
		}
		
		public static void send (Connection connection, Packet packet)
		{
			Stream stream = connection.getNetworkStream();
			packet.write (stream);
		}


		public void enQueue (Call call)
		{
			Console.WriteLine("EnQ");
			inQ.Add(call);
		}

		public OutThread start (String prefix)
		{
			Thread thread = new Thread (new ThreadStart (this.run));
			thread.Name = prefix + "OutThread";
			thread.Start ();
			return this;
		}
		
		public static void write(Connection connection, Packet packet){
			if (connection != null) {
	            Stream stream = connection.getNetworkStream();
	            if (!connection.headersWritten) {
	                stream.Write(Packet.HEADER, 0, Packet.HEADER.Length);
	                connection.headersWritten = true;
	            }
	            send (connection, packet);
			}
		}
		
		public void shutdown(){
			if(connection!=null)
				connection.close();	
		}
	}
}

