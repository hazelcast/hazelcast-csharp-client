// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.Util;

 namespace Hazelcast.Client.Spi
{
    class ClientInvocation
    {
        public ClientInvocation(IClientMessage message, IFuture<IClientMessage> future)
        {
            Message = message;
            Future = future;
            InvocationTimeMillis = Clock.CurrentTimeMillis();
        }

        public ClientInvocation(IClientMessage message, IFuture<IClientMessage> future, int partitionId) : this(message, future)
        {
            PartitionId = partitionId;
        }

        public ClientInvocation(IClientMessage message, IFuture<IClientMessage> future, string memberUuid) : this(message, future)
        {
            MemberUuid = memberUuid;
        }

        public ClientInvocation(IClientMessage message, IFuture<IClientMessage> future, Address address) : this(message, future)
        {
            Address = address;
        }

        public ClientInvocation(IClientMessage message, IFuture<IClientMessage> future, ClientConnection boundConnection, DistributedEventHandler eventHandler = null)
            : this(message, future)
        {
            BoundConnection = boundConnection;
            EventHandler = eventHandler;
        }

        public string MemberUuid { get; }

        public int PartitionId { get; } = -1;

        public IClientMessage Message { get; }

        public IFuture<IClientMessage> Future { get; }

        /// <summary>
        /// Connection that was used to execute this invocation
        /// </summary>
        public ClientConnection SentConnection { get; set; }

        public ClientConnection BoundConnection { get; }

        public Address Address { get; private set; }

        // the point at which the request should be considered timed out
        public long InvocationTimeMillis { get; }

        public DistributedEventHandler EventHandler { get; }
    }
}