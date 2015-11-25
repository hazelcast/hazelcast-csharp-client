// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

        public virtual ClientNetworkConfig AddAddress(params string[] addresses)
        {
            _addressList.AddRange(addresses);
            return this;
        }

        public virtual IList<string> GetAddresses()
        {
            if (_addressList.Count == 0)
            {
                AddAddress("localhost");
            }
            return _addressList;
        }

        public virtual int GetConnectionAttemptLimit()
        {
            return _connectionAttemptLimit;
        }

        public virtual int GetConnectionAttemptPeriod()
        {
            return _connectionAttemptPeriod;
        }

        public virtual int GetConnectionTimeout()
        {
            return _connectionTimeout;
        }

        public virtual SocketInterceptorConfig GetSocketInterceptorConfig()
        {
            return _socketInterceptorConfig;
        }

        public virtual SocketOptions GetSocketOptions()
        {
            return _socketOptions;
        }

        public virtual bool IsRedoOperation()
        {
            return _redoOperation;
        }

        /// <summary>Enabling ssl for client</summary>
        //private SSLConfig sslConfig = null;
        public virtual bool IsSmartRouting()
        {
            return _smartRouting;
        }

        // required for spring module
        public virtual ClientNetworkConfig SetAddresses(IList<string> addresses)
        {
            _addressList.Clear();
            _addressList.AddRange(addresses);
            return this;
        }

        public virtual ClientNetworkConfig SetConnectionAttemptLimit(int connectionAttemptLimit)
        {
            _connectionAttemptLimit = connectionAttemptLimit;
            return this;
        }

        public virtual ClientNetworkConfig SetConnectionAttemptPeriod(int connectionAttemptPeriod)
        {
            _connectionAttemptPeriod = connectionAttemptPeriod;
            return this;
        }

        public virtual ClientNetworkConfig SetConnectionTimeout(int connectionTimeout)
        {
            _connectionTimeout = connectionTimeout;
            return this;
        }

        public virtual ClientNetworkConfig SetRedoOperation(bool redoOperation)
        {
            _redoOperation = redoOperation;
            return this;
        }

        public virtual ClientNetworkConfig SetSmartRouting(bool smartRouting)
        {
            _smartRouting = smartRouting;
            return this;
        }

        public virtual ClientNetworkConfig SetSocketInterceptorConfig(SocketInterceptorConfig socketInterceptorConfig)
        {
            _socketInterceptorConfig = socketInterceptorConfig;
            return this;
        }

        public virtual ClientNetworkConfig SetSocketOptions(SocketOptions socketOptions)
        {
            _socketOptions = socketOptions;
            return this;
        }
    }
}