// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains configuration parameters for client network related behavior
    /// </summary>
    public class ClientNetworkConfig
    {
        /// <summary>List of the initial set of addresses.</summary>
        /// <remarks>
        /// List of the initial set of addresses.
        /// Client will use this list to find a running Member, connect to it.
        /// </remarks>
        private readonly List<string> _addressList = new List<string>(10);

        /// <summary>
        /// While client is trying to connect initially to one of the members in the
        /// <see cref="_addressList">addressList</see>
        /// ,
        /// all might be not available. Instead of giving up, throwing Exception and stopping client, it will
        /// attempt to retry as much as
        /// <see cref="_connectionAttemptLimit">connectionAttemptLimit</see>
        /// times.
        /// </summary>
        private int _connectionAttemptLimit = 2;

        /// <summary>Period for the next attempt to find a member to connect.</summary>
        /// <remarks>
        /// Period for the next attempt to find a member to connect. (see
        /// <see cref="_connectionAttemptLimit">connectionAttemptLimit</see>
        /// ).
        /// </remarks>
        private int _connectionAttemptPeriod = 3000;

        /// <summary>Timeout value in millis for nodes to accept client connection requests</summary>
        /// <remarks>
        /// Timeout value in millis for nodes to accept client connection requests
        /// </remarks>
        private int _connectionTimeout = 5000;

        /// <summary>If true, client will redo the operations that were executing on the server and client lost the connection.</summary>
        /// <remarks>
        /// If true, client will redo the operations that were executing on the server and client lost the connection.
        /// This can be because of network, or simply because the member died. However it is not clear whether the
        /// application is performed or not. For idempotent operations this is harmless, but for non idempotent ones
        /// retrying can cause to undesirable effects. Note that the redo can perform on any member.
        /// <p/>
        /// </remarks>
        private bool _redoOperation;

        /// <summary>If true, client will route the key based operations to owner of the key at the best effort.</summary>
        /// <remarks>
        /// If true, client will route the key based operations to owner of the key at the best effort.
        /// Note that it  doesn't guarantee that the operation will always be executed on the owner. The cached table is updated every 10 seconds.
        /// </remarks>
        private bool _smartRouting = true;

        /// <summary>Will be called with the Socket, each time client creates a connection to any Member.</summary>
        /// <remarks>Will be called with the Socket, each time client creates a connection to any Member.</remarks>
        private SocketInterceptorConfig _socketInterceptorConfig;

        /// <summary>Options for creating socket</summary>
        private SocketOptions _socketOptions = new SocketOptions();

        private SSLConfig _sslConfig = new SSLConfig();
        
        private ClientCloudConfig _cloudConfig = new ClientCloudConfig();


        /// <summary>
        /// Adds given addresses to the list of candidate addresses that client will use to establish initial connection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig AddAddress(params string[] addresses)
        {
            _addressList.AddRange(addresses);
            return this;
        }

        /// <summary>
        /// Returns the list of candidate addresses that client will use to establish initial connection
        /// </summary>
        /// <returns>list of addresses</returns>
        public virtual IList<string> GetAddresses()
        {
            return _addressList;
        }

        /// <summary>
        /// While client is trying to connect initially to one of the members in the configured address list,
        /// all might be not available. Instead of giving up, throwing Exception and stopping client, it will
        /// attempt to retry as much as connection attempt limit times.
        /// </summary>
        /// <returns>number of times to attempt to connect</returns>
        public virtual int GetConnectionAttemptLimit()
        {
            return _connectionAttemptLimit;
        }

        /// <summary>
        /// Gets the connection attempt period, in millis, for the next attempt to find a member to connect.
        /// </summary>
        /// <returns>connection attempt period in millis</returns>
        public virtual int GetConnectionAttemptPeriod()
        {
            return _connectionAttemptPeriod;
        }

        /// <summary>
        /// Gets connection timeout.
        /// </summary>
        /// <returns>connection timeout</returns>
        public virtual int GetConnectionTimeout()
        {
            return _connectionTimeout;
        }

        /// <summary>
        /// Gets <see cref="ClientCloudConfig"/>
        /// </summary>
        /// <returns><see cref="ClientCloudConfig"/></returns>
        public ClientCloudConfig GetCloudConfig() {
            return _cloudConfig;
        }

        /// <summary>
        /// Gets <see cref="SocketInterceptorConfig"/>.
        /// </summary>
        /// <returns><see cref="SocketInterceptorConfig"/></returns>
        public virtual SocketInterceptorConfig GetSocketInterceptorConfig()
        {
            return _socketInterceptorConfig;
        }

        /// <summary>
        /// Gets <see cref="SocketOptions"/>.
        /// </summary>
        /// <returns><see cref="SocketOptions"/></returns>
        public virtual SocketOptions GetSocketOptions()
        {
            return _socketOptions;
        }

        /// <summary>
        /// Gets <see cref="SSLConfig"/>.
        /// </summary>
        /// <returns><see cref="SSLConfig"/></returns>
        public virtual SSLConfig GetSSLConfig()
        {
            return _sslConfig;
        }

        /// <summary>
        /// Specifies whether the redo operations are enabled or not.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRedoOperation()
        {
            return _redoOperation;
        }

        /// <summary>
        /// Specifies whether the client is smart or not.
        /// </summary>
        /// <returns><c>true</c> if client is smart</returns>
        public virtual bool IsSmartRouting()
        {
            return _smartRouting;
        }

        /// <summary>
        /// Sets all addresses
        /// </summary>
        /// <param name="addresses">list of addresses</param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetAddresses(IList<string> addresses)
        {
            _addressList.Clear();
            _addressList.AddRange(addresses);
            return this;
        }

        /// <summary>
        /// Sets <see cref="ClientCloudConfig"/>
        /// </summary>
        /// <param name="cloudConfig"><see cref="ClientCloudConfig"/></param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public ClientNetworkConfig SetCloudConfig(ClientCloudConfig cloudConfig) 
        {
            _cloudConfig = cloudConfig;
            return this;
        }


        /// <summary>
        /// While client is trying to connect initially to one of the members in the configured address list,
        /// all might be not available. Instead of giving up, throwing Exception and stopping client, it will
        /// attempt to retry as much as connection attempt limit times.
        /// </summary>
        /// <param name="connectionAttemptLimit">
        /// number of times to attempt to connect;
        /// A zero value means try forever.
        /// A negative value means default value
        /// </param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetConnectionAttemptLimit(int connectionAttemptLimit)
        {
            _connectionAttemptLimit = connectionAttemptLimit;
            return this;
        }

        /// <summary>
        /// Sets the connection attempt period, in millis, for the next attempt to find a member to connect..
        /// </summary>
        /// <param name="connectionAttemptPeriod">time to wait before another attempt in millis</param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetConnectionAttemptPeriod(int connectionAttemptPeriod)
        {
            _connectionAttemptPeriod = connectionAttemptPeriod;
            return this;
        }

        /// <summary>
        /// Sets connection timeout
        /// </summary>
        /// <param name="connectionTimeout">connection timeout</param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetConnectionTimeout(int connectionTimeout)
        {
            _connectionTimeout = connectionTimeout;
            return this;
        }

        /// <summary>
        /// If true, client will redo the operations that were executing on the server and client lost the connection.
        /// This can be because of network, or simply because the member died. However it is not clear whether the
        /// application is performed or not. For idempotent operations this is harmless, but for non-idempotent ones
        /// retrying can cause to undesirable effects. Note that the redo can perform on any member.
        /// If false, the operation will throw exception
        /// </summary>
        /// <param name="redoOperation">true if redo operations are enabled</param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetRedoOperation(bool redoOperation)
        {
            _redoOperation = redoOperation;
            return this;
        }

        /// <summary>
        /// If true, client will route the key based operations to owner of the key at the best effort.
        /// </summary>
        /// <param name="smartRouting">true if smart routing should be enabled.</param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetSmartRouting(bool smartRouting)
        {
            _smartRouting = smartRouting;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SocketInterceptorConfig"/>
        /// </summary>
        /// <param name="socketInterceptorConfig"><see cref="SocketInterceptorConfig"/></param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetSocketInterceptorConfig(SocketInterceptorConfig socketInterceptorConfig)
        {
            _socketInterceptorConfig = socketInterceptorConfig;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SocketOptions"/>
        /// </summary>
        /// <param name="socketOptions"><see cref="SocketOptions"/></param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetSocketOptions(SocketOptions socketOptions)
        {
            _socketOptions = socketOptions;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SSLConfig"/>
        /// </summary>
        /// <param name="sslConfig"><see cref="SSLConfig"/></param>
        /// <returns><see cref="ClientNetworkConfig"/> for chaining</returns>
        public virtual ClientNetworkConfig SetSSLConfig(SSLConfig sslConfig)
        {
            _sslConfig = sslConfig;
            return this;
        }
     }
}