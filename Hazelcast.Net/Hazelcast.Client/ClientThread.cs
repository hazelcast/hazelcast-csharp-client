using System;
using System.Threading;

namespace Hazelcast.Client
{
	public abstract class ClientThread
	{
		
		protected volatile bool running = true;
    	protected volatile bool terminated = false;
		protected readonly Object monitor = new Object();
		
		public ClientThread ()
		{
		}
		
		protected abstract void customRun();
		
		public void run() {
            try
            {
                while (running)
                {
                    customRun();
                }
            }
            finally {
	            terminate();
	        }
		}	
		public void shutdown() {
	        if(terminated){
	            return;
	        }

	        lock (monitor) {
	            running = false;
	            while (!terminated) {
	                Monitor.Wait(monitor);
	            }
	        }
		}

	    protected void terminate() {
	        lock (monitor) {
	            terminated = true;
	            Monitor.PulseAll(monitor);
	        }
	    }
	}
}

