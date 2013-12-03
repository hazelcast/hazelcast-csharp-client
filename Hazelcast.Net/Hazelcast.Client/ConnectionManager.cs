using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Hazelcast.Security;
using Hazelcast.Core;
using System.Diagnostics;

namespace Hazelcast.Client
{
	public class ConnectionManager
	{
		private long TIMEOUT;
	    private volatile Connection currentConnection;
	    private int connectionIdGenerator = -1;
	    private List<IPEndPoint> clusterMembers = new List<IPEndPoint>();
	    //private ILogger logger = Logger.getLogger(getClass().getName());
	    private HazelcastClient client;
	    private volatile int lastDisconnectedConnectionId = -1;
	    private ClientBinder binder;
	    private ClientConfig config;
	
	    private volatile bool lookingForLiveConnection = false;
	    private volatile bool running = true;
	
	    private LifecycleServiceClientImpl lifecycleService;
	    Timer heartbeatTimer = null;
		
	
	    public ConnectionManager(HazelcastClient client, ClientConfig config, LifecycleServiceClientImpl lifecycleService) {
	        this.TIMEOUT = config.ConnectionTimeout;
	        this.client = client;
	        this.lifecycleService = lifecycleService;
			this.config = config;
			foreach (IPEndPoint ip in config.AddressList){
				clusterMembers.Add(ip);
			}
	    }
	
	    void scheduleHeartbeatTimerTask() {
			ConnectionChecker connectionChecker = new ConnectionChecker(client, TIMEOUT, currentConnection);
			heartbeatTimer = new Timer(connectionChecker.checkConnection, null, TIMEOUT / 10, TIMEOUT / 10 );
	    }
				
	 class ConnectionChecker{
			
			HazelcastClient client;
			long timeout;
			Connection currentConnection;
			public ConnectionChecker(HazelcastClient client, long timeout, Connection currentConnection){
				this.client = client;
				this.timeout = timeout;
				this.currentConnection = currentConnection;
			}
			
			public void checkConnection(object state){
				 
		        long diff = (client.InThread.lastReceived - System.DateTime.Now.Ticks)/10000;
		        
	            if (diff >= timeout / 5 && diff < timeout) {
	                //logger.log(Level.FINEST, "Being idle for some time, Doing a getMembers() call to ping the server!");
	                CountdownEvent latch = new CountdownEvent(1);
					ThreadPool.QueueUserWorkItem(
					(obj) => 
					{
						System.Collections.Generic.ICollection<Member> members = client.getCluster().getMembers();
	                    if (members != null && members.Count >= 1) {
	                            latch.Signal();
	                        }
					});
					
	                if (!latch.Wait(10000)) {
	                    //logger.log(Level.WARNING, "Server didn't respond to client's ping call within 10 seconds!");
	                }
	            } else if (diff >= timeout) {
	                //logger.log(Level.WARNING, "Server didn't respond to client's requests for " + TIMEOUT / 1000 + " seconds. Assuming it is dead, closing the connection!");
	                currentConnection.close();
	            }
	       
	    	}
		}
	    
		public Connection getInitConnection(){


	        if (currentConnection == null) {
	            lock (this) {
					int attemptsLimit = client.ClientConfig.InitialConnectionAttemptLimit;
					int reconnectionTimeout = client.ClientConfig.ReConnectionTimeOut;
	                currentConnection = lookForLiveConnection(attemptsLimit, reconnectionTimeout);
				}
	        }
	        return currentConnection;
	    }
	
	    public Connection lookForLiveConnection() {
	        int attemptsLimit = client.ClientConfig.ReconnectionAttemptLimit;
	        int reconnectionTimeout = client.ClientConfig.ReConnectionTimeOut;
	        return lookForLiveConnection(attemptsLimit, reconnectionTimeout);
	    }
	
	    private Connection lookForLiveConnection(int attemptsLimit, int reconnectionTimeout) {
	        lookingForLiveConnection = true;
	        try {
	            bool restored = false;
	            int attempt = 0;
	            while (currentConnection == null && running) {
	                System.DateTime next = System.DateTime.Now.AddMilliseconds(reconnectionTimeout);
	                lock (this) {
	                    if (currentConnection == null) {
	                        Connection connection = searchForAvailableConnection();
	                        restored = connection != null;
	                        if (restored) {
	                            try {

									if (config != null) {
										SocketInterceptor socketInterceptor = config.SocketInterceptor;
										if (socketInterceptor != null) {
											socketInterceptor.onConnect(connection.getSocket());
										}
									}
	                                bindConnection(connection);
	                                currentConnection = connection;
	                            } catch (Exception e) {
	                                closeConnection(connection);
	                                Console.WriteLine("got an exception on getConnection: " + e.Message + "\n" + e.StackTrace);
	                                restored = false;
	                            }
	                        }
	                    }
	                }
	                if (currentConnection != null) {
	                    //logger.log(Level.FINE, "Client is connecting to " + currentConnection);
	                    lookingForLiveConnection = false;
	                    break;
	                }
	                if (attempt >= attemptsLimit) {
	                    break;
	                }
	                attempt++;
	                double t = (next - System.DateTime.Now).TotalMilliseconds;
	                //logger.log(Level.INFO, format("Unable to get alive cluster connection," +
	                //        " try in {0} ms later, attempt {1} of {2}.",
	                //        Math.max(0, t), attempt, attemptsLimit));
	                if (t > 0) {
	                    Thread.Sleep((int)t);
	                }
	            }
	            if (restored) {
	                notifyConnectionIsRestored();
	            }
	        } finally {
	            lookingForLiveConnection = false;
	        }
	        return currentConnection;
	    }
	
	    void closeConnection(Connection connection) {
	        try {
	            if (connection != null) {
	                connection.close();
	            }
	        } catch (Exception e) {
	            //logger.log(Level.INFO, "got an exception on closeConnection "
	            //        + connection + ":" + e.getMessage(), e);
	        }
	    }
	
	    public Connection getConnection() {
	        if (currentConnection == null && running && !lookingForLiveConnection) {
	            bool restored = false;
	            lock (this) {
	                if (currentConnection == null) {
	                    Connection connection = searchForAvailableConnection();
	                    if (connection != null) {
	                        //logger.log(Level.FINE, "Client is connecting to " + connection);
	                        try {
	                            bindConnection(connection);
	                            currentConnection = connection;
	                        } catch (Exception e) {
	                            closeConnection(connection);
	                            //logger.log(Level.WARNING, "got an exception on getConnection:" + e.getMessage(), e);
	                        }
	                    }
	                    restored = currentConnection != null;
	                }
	            }
	            if (restored) {
	                notifyConnectionIsRestored();
	            }
	        }
	        return currentConnection;
	    }
	
	    void notifyConnectionIsRestored() {
	        //lifecycleService.fireLifecycleEvent(CLIENT_CONNECTION_OPENING);
	    }
	
	    void notifyConnectionIsOpened() {
	        /*notify(new Runnable() {
	            public void run() {
	                lifecycleService.fireLifecycleEvent(CLIENT_CONNECTION_OPENED);
	            }
	        });*/
	    }
	
	    //private void notify(Runnable target) {
	    //    client.runAsyncAndWait(target);
	    //}
	
	    void bindConnection(Connection connection) {
	        binder.bind(connection, config.Credentials);
	    }
	
	    public void destroyConnection(Connection connection) {
	        bool lost = false;
	        lock(this) {
	            if (currentConnection != null &&
	                    connection != null &&
	                    currentConnection.getVersion() == connection.getVersion()) {
	                //logger.log(Level.WARNING, "Connection to " + currentConnection + " is lost");
	                currentConnection = null;
	                lost = true;
	                try {
	                    connection.close();
	                } catch (Exception e) {
	                    //logger.log(Level.FINEST, e.getMessage(), e);
	                }
	            }
	        }
	        if (lost) {
	            /*notify(new Runnable() {
	                public void run() {
	                    lifecycleService.fireLifecycleEvent(CLIENT_CONNECTION_LOST);
	                }
	            });*/
	        }
	    }
	
	    private void popAndPush(List<IPEndPoint> clusterMembers) {
	    	IPEndPoint address = clusterMembers[0];
        	clusterMembers.RemoveAt(0);
			clusterMembers.Add(address);  
	    }
	
	    private Connection searchForAvailableConnection() {
			 lock(this.clusterMembers){
		        Connection connection = null;
		        popAndPush(clusterMembers);
		        int counter = clusterMembers.Count;
		        while (counter > 0) {
		            try {
		                connection = getNextConnection();
		                break;
		            } catch (Exception e) {
		                popAndPush(clusterMembers);
		                counter--;
		            }
		        }
	        //logger.log(Level.FINEST, format("searchForAvailableConnection connection:{0}", connection));
	        return connection;
			}
	    }
	
	    protected Connection getNextConnection() {
			lock(clusterMembers){
	        	IPEndPoint address = clusterMembers[0];
	        	return new Connection(address, incrementLastDisconnectedConnectionId());
			}
	    }
	
	    public void memberAdded(MembershipEvent membershipEvent) {
	        lock(this){
				lock(clusterMembers){
					if (!this.clusterMembers.Contains(membershipEvent.getMember().getIPEndPoint())) {
		           		this.clusterMembers.Add(membershipEvent.getMember().getIPEndPoint());
		        	}
				}
			}
	    }
	
	    public void memberRemoved(MembershipEvent membershipEvent) {
	        lock(this){
				lock(clusterMembers){
					this.clusterMembers.Remove(membershipEvent.getMember().getIPEndPoint());
				}
			}
		}
	
	    public void updateMembers() {
	        lock(this){
				lock(clusterMembers){
					System.Collections.Generic.ICollection<Member> members = client.getCluster().getMembers();
			        clusterMembers.Clear();
			        foreach (Member member in members) {
			            clusterMembers.Add(member.getIPEndPoint());
			        }
				}
			}
	    }
	
	    public bool shouldExecuteOnDisconnect(Connection connection) {
	        if (connection == null || lastDisconnectedConnectionId >= connection.getVersion()) {
	            return false;
	        }
	        lastDisconnectedConnectionId = connection.getVersion();
	        return true;
	    }
	
	    public void setBinder(ClientBinder binder) {
	        this.binder = binder;
	    }
	
	    List<IPEndPoint> getClusterMembers() {
	        return clusterMembers;
	    }
	
	    public void shutdown() {
	        //logger.log(Level.INFO, getClass().getSimpleName() + " shutdown");
	        running = false;
			if(heartbeatTimer!=null)
	        	heartbeatTimer.Dispose();
	    }
		
		private int incrementLastDisconnectedConnectionId ()
		{
			int initialValue, computedValue;
			do {
				initialValue = lastDisconnectedConnectionId;
				computedValue = initialValue + 1;
				
			} while (initialValue != Interlocked.CompareExchange (ref lastDisconnectedConnectionId, computedValue, initialValue));
			return computedValue;
		}
	}
}

