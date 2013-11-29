using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    public class ConnectionWrapper : IConnection
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (ConnectionWrapper));

        private readonly IConnection _connection;
        private readonly SmartClientConnectionManager _enclosing;

        internal ConnectionWrapper(SmartClientConnectionManager enclosing, IConnection connection)
        {
            _enclosing = enclosing;
            _connection = connection;
        }

        public virtual Address GetRemoteEndpoint()
        {
            return _connection.GetRemoteEndpoint();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool Write(Data data)
        {
            return _connection.Write(data);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual Data Read()
        {
            return _connection.Read();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Release()
        {
            _enclosing.ReleaseConnection(this);
        }

        public virtual void Close()
        {
            logger.Info("Closing connection -> " + _connection);
            IOUtil.CloseResource(_connection);
        }

        public virtual int GetId()
        {
            return _connection.GetId();
        }

        public virtual long GetLastReadTime()
        {
            return _connection.GetLastReadTime();
        }

        public virtual void SetRemoteEndpoint(Address address)
        {
            _connection.SetRemoteEndpoint(address);
        }

        public virtual IPEndPoint GetLocalSocketAddress()
        {
            return _connection.GetLocalSocketAddress();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public override string ToString()
        {
            return _connection.ToString();
        }
    }

    public class SmartClientConnectionManager : IClientConnectionManager
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (IClientConnectionManager));

        private readonly ConcurrentDictionary<Address, IObjectPool<ConnectionWrapper>>
            _connPool = new ConcurrentDictionary<Address, IObjectPool<ConnectionWrapper>>();

        private readonly Authenticator authenticator;

        private readonly HazelcastClient client;
        private readonly HeartBeatChecker heartbeat;
        private readonly int poolSize;

        private readonly Router router;

        private readonly SocketInterceptor socketInterceptor;
        private readonly SocketOptions socketOptions;

        private volatile bool live = true;

        public SmartClientConnectionManager(HazelcastClient client, Authenticator authenticator,
            LoadBalancer loadBalancer)
        {
            this.authenticator = authenticator;
            this.client = client;
            ClientConfig config = client.GetClientConfig();
            router = new Router(loadBalancer);
            //init socketInterceptor
            SocketInterceptorConfig sic = config.GetSocketInterceptorConfig();
            if (sic != null && sic.IsEnabled())
            {
                var implementation = (SocketInterceptor) sic.GetImplementation();
                if (implementation == null && sic.GetClassName() != null)
                {
                    try
                    {
                        Type type = Type.GetType(sic.GetClassName());
                        if (type != null)
                        {
                            implementation = (SocketInterceptor) Activator.CreateInstance(type);
                        }
                        else
                        {
                            throw new NullReferenceException("SocketInterceptor class is not found");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Severe("SocketInterceptor class cannot be instantiated!" + sic.GetClassName(), e);
                    }
                }
                if (implementation != null)
                {
                    if (!(implementation is IMemberSocketInterceptor))
                    {
                        logger.Severe("SocketInterceptor must be instance of " +
                                      typeof (IMemberSocketInterceptor).FullName);
                        implementation = null;
                    }
                    else
                    {
                        logger.Info("SocketInterceptor is enabled");
                    }
                }
                if (implementation != null)
                {
                    socketInterceptor = implementation;
                    socketInterceptor.Init(sic.GetProperties());
                }
                else
                {
                    socketInterceptor = null;
                }
            }
            else
            {
                socketInterceptor = null;
            }
            poolSize = config.GetConnectionPoolSize();
            int connectionTimeout = config.GetConnectionTimeout();
            heartbeat = new HeartBeatChecker(connectionTimeout, client.GetSerializationService(),
                client.GetClientExecutionService());
            socketOptions = config.GetSocketOptions();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IConnection FirstConnection(Address address, Authenticator authenticator)
        {
            return NewConnection(address, authenticator);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IConnection NewConnection(Address address, Authenticator authenticator)
        {
            CheckLive();
            var connection = new Connection(address, socketOptions, client.GetSerializationService());
            if (socketInterceptor != null)
            {
                socketInterceptor.OnConnect(connection.GetSocket());
            }
            connection.Init();
            authenticator(connection);
            return connection;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IConnection GetRandomConnection()
        {
            CheckLive();
            Address address = router.Next();
            if (address == null)
            {
                throw new IOException("LoadBalancer '" + router + "' could not find a address to route to");
            }
            return GetConnection(address);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IConnection GetConnection(Address address)
        {
            CheckLive();
            if (address == null)
            {
                throw new ArgumentException("Target address is required!");
            }
            IObjectPool<ConnectionWrapper> pool = GetConnectionPool(address);
            if (pool == null)
            {
                return null;
            }
            IConnection connection = null;
            try
            {
                connection = pool.Take();
            }
            catch (Exception e)
            {
                if (logger.IsFinestEnabled())
                {
                    logger.Warning("Error during connection creation... To -> " + address, e);
                }
            }
            // Could be that this address is dead and that's why pool is not able to create and give a connection.
            // We will call it again, and hopefully at some time LoadBalancer will give us the right target for the connection.
            if (connection != null && !heartbeat.CheckHeartBeat(connection))
            {
                logger.Warning(connection + " failed to heartbeat, closing...");
                connection.Close();
                connection = null;
            }
            return connection;
        }

        public virtual void RemoveConnectionPool(Address address)
        {
            IObjectPool<ConnectionWrapper> pool = null;
            _connPool.TryRemove(address, out pool);
            if (pool != null)
            {
                pool.Destroy();
            }
        }

        public virtual void Shutdown()
        {
            live = false;
            foreach (var pool in _connPool.Values)
            {
                pool.Destroy();
            }
            _connPool.Clear();
        }

        private void CheckLive()
        {
            if (!live)
            {
                throw new HazelcastInstanceNotActiveException();
            }
        }

        private IObjectPool<ConnectionWrapper> CreateNew(Address address)
        {
            return new QueueBasedObjectPool<ConnectionWrapper>(poolSize,
                delegate { return new ConnectionWrapper(this, NewConnection(address, authenticator)); }, //
                delegate(ConnectionWrapper wrapper)
                {
                    wrapper.Close();
                    return wrapper;
                });
        }


        private IObjectPool<ConnectionWrapper> GetConnectionPool(Address address)
        {
            CheckLive();
            IObjectPool<ConnectionWrapper> pool = null;
            _connPool.TryGetValue(address, out pool);
            if (pool == null)
            {
                if (client.GetClientClusterService().GetMember(address) == null)
                {
                    return null;
                }
                pool = CreateNew(address);
                _connPool.TryAdd(address, pool);
            }
            return pool;
        }

        internal void ReleaseConnection(ConnectionWrapper connection)
        {
            if (live)
            {
                IObjectPool<ConnectionWrapper> pool = null;
                _connPool.TryGetValue(connection.GetRemoteEndpoint(), out pool);
                if (pool != null)
                {
                    pool.Release(connection);
                }
                else
                {
                    connection.Close();
                }
            }
            else
            {
                connection.Close();
            }
        }

        private delegate void Del(int x);
    }
}