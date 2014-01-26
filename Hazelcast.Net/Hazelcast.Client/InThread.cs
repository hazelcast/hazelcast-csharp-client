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
        private HazelcastClient client;
		public Int64 lastReceived;
		
		
		public InThread (ConnectionManager connectionManager, ConcurrentDictionary<long, Call> calls, HazelcastClient client)
		{
			this.connectionManager = connectionManager;
			this.calls = calls;
			this.listenerManager = client.ListenerManager;
            this.client = client;
		}
		
		protected override void customRun(){
            try
            {
                connection = connectionManager.getConnection();
                if (connection == null)
                    throw new Exception("Cluster is down!");

                Packet packet = readPacket(this.connection);
                if (packet == null)
                    return;
                Interlocked.Exchange(ref lastReceived, DateTime.Now.Ticks);

                Call call;
                if (calls.TryRemove(packet.callId, out call))
                {
                    //call.on.Stop();
                    //call.post.Start();
                    //Console.WriteLine("Received Answer for " + call.getId());
                    call.setResult(packet);
                }
                else
                {
                    if (packet.operation == (byte)ClusterOperation.EVENT)
                        listenerManager.enQueue(packet);
                    //else Console.WriteLine("Unkown call result: " + packet.callId + ", " + packet.operation);
                }
            }
            catch (Exception e)
            {
                if (!running || terminated) 
                    return;

               new Thread(client.getLifecycleService().shutdown).Start();
               //Console.WriteLine("Exception on Socket, terminating all existing calls (" + calls.Count + ") and shutting down the client");
                
                foreach (long id in calls.Keys)
                {
                    Call call;
                    calls.TryRemove(id, out call);
                    call.setResult(new Exception("Connection is broken!"));
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

        public void shutdown() {
        lock (monitor) {
            if (running) {
                this.running = false;
                try {
                    if (connection != null) {
                        connection.close();
                    }
                } catch (IOException ignored) {
                }
                    Monitor.Wait(monitor,5000);
            }
        }
    }
	}
}
