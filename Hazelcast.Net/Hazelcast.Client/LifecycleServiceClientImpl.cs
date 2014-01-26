using System;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class LifecycleServiceClientImpl : LifecycleService
	{
		//static ILogger logger = Logger.getLogger(LifecycleServiceClientImpl.class.getName());
	    bool paused = false;
	    bool running = true;
	    //CopyOnWriteArrayList<LifecycleListener> lsLifecycleListeners = new CopyOnWriteArrayList<LifecycleListener>();
	    Object lifecycleLock = new Object();
	    HazelcastClient hazelcastClient;
	
	    public LifecycleServiceClientImpl(HazelcastClient hazelcastClient) {
	        this.hazelcastClient = hazelcastClient;
	    }
	
		/* 
	    public void addLifecycleListener(LifecycleListener lifecycleListener) {
	       lsLifecycleListeners.add(lifecycleListener);
	    }
	
	   public void removeLifecycleListener(LifecycleListener lifecycleListener) {
	        lsLifecycleListeners.remove(lifecycleListener);
	    }
	
	    public void fireLifecycleEvent(LifecycleState lifecycleState) {
	        fireLifecycleEvent(new LifecycleEvent(lifecycleState));
	    }
	
	    public void fireLifecycleEvent(LifecycleEvent event) {
	        logger.log(Level.INFO, "HazelcastClient is " + event.getState());
	        for (LifecycleListener lifecycleListener : lsLifecycleListeners) {
	            lifecycleListener.stateChanged(event);
	        }
	    }*/
	
	    public bool resume() {
	        throw new Exception("Operation is unsupported!");
	    }
	
	    public bool pause() {
            throw new Exception("Operation is unsupported!");
	    }
	
	    public void shutdown() {
			lock (lifecycleLock) {
			    DateTime begin = System.DateTime.Now;
			    //fireLifecycleEvent(SHUTTING_DOWN);
			    hazelcastClient.doShutdown();
			    running = false;
			    double time = (System.DateTime.Now - begin).TotalMilliseconds;
			    //Console.WriteLine("HazelcastClient shutdown completed in " + time + " ms.");
			    //fireLifecycleEvent(SHUTDOWN);
			}
	    }
	
	    public void kill() {
	        shutdown();
	    }
	
	    public void restart() {
	        throw new Exception("Operation is unsupported!");
	    }
	
	    public bool isRunning() {
	        lock(lifecycleLock){
				return running;
			}
	    }
	}
}

