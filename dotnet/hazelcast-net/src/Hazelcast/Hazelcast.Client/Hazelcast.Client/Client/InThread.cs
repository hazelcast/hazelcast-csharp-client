using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Hazelcast.Client.Impl;
using System.IO;


namespace Hazelcast.Client
{
	public class InThread : ClientThread
	{
		
		private ConnectionManager connectionManager;
		private Connection connection;
		private ConcurrentDictionary<long, Call> calls;
		private ListenerManager listenerManager;		
		public Int64 lastReceived;
		
		
		public InThread (ConnectionManager connectionManager, ConcurrentDictionary<long, Call> calls, ListenerManager listenerManager)
		{
			this.connectionManager = connectionManager;
			this.calls = calls;
			this.listenerManager = listenerManager;
		}
		
		protected override void customRun(){
			connection = connectionManager.getConnection();
			if(connection == null){
				throw new Exception("Cluster is down!");
			}
			Packet packet = readPacket(this.connection);
			if(packet == null)
				return;
			System.Threading.Interlocked.Exchange(ref lastReceived, DateTime.Now.Ticks);
			
			Call call;
			if(calls.TryGetValue(packet.callId, out call)){
				call.setResult(packet);	
			}
			else{
				if(packet.operation == (byte)ClusterOperation.EVENT){
					listenerManager.enQueue(packet);		
				} 
				else
				{
					Console.WriteLine("Unkown call result: " + packet.callId + ", " + packet.operation);
				}
				
			}
		}
		
		public static bool equals(byte[] b1, byte[] b2){
			if(b1.Length!=b2.Length){
				return false;
			}
			for(int i=0;i<b1.Length;i++){
				if(b1[i]!=b2[i]){
					return false;
				}
			}
			
			return true;
			
		}
		
		public Packet readPacket(Connection connection){
        	Stream stream = connection.getNetworkStream();
			if(!connection.headerRead)
			{
				byte[] header = new byte[3];
				stream.Read(header, 0, 3);
				if(equals(header, Packet.HEADER)){
					
				}
				connection.headerRead = true;
			}
			
			if(!stream.CanRead){
				Thread.Sleep(100);
				return null;
			}
			
			Packet packet = new Packet();
			packet.read(stream);	
			return packet;
    	}
		
		
		
		
		public InThread start(String prefix)
		{
			Thread thread =  new Thread(new ThreadStart(this.run));
			thread.Name = prefix + "InThread";
			thread.Start();
			return this;
		}
	}
}
