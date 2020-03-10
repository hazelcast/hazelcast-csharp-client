// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Diagnostics;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    [DebuggerDisplay("")]
    internal class Invocation
    {
        private readonly AtomicInteger _counter = new AtomicInteger();
        public ClientMessage Request { get; private set; }

        public Guid? TargetUuid { get; }

        public int PartitionId { get; }

        public DistributedEventHandler EventHandler { get; }

        public SettableFuture<ClientMessage> Future { get; }

        public Connection SentConnection { get; set; }

        public Connection BoundConnection { get; }

        public long StartTimeMillis { get; }

        public int InvokeCount => _counter.Get();

        public string ObjectName { get; }

        public int IncrementCounter()
        {
            return _counter.IncrementAndGet();
        }
        
        public bool IsBindToSingleConnection  => BoundConnection != null;

        public Invocation(ClientMessage request, Guid? targetUuid = null, int partitionId = -1,
            Connection boundConnection = null, DistributedEventHandler eventHandler = null, string objectName = null)
        {
            Request = request;
            TargetUuid = targetUuid;
            PartitionId = partitionId;
            BoundConnection = boundConnection;
            EventHandler = eventHandler;
            ObjectName = objectName;
            Future = new SettableFuture<ClientMessage>();
            StartTimeMillis = Clock.CurrentTimeMillis();
            if (PartitionId != -1)
            {
                Request.PartitionId = PartitionId;
            }
        }

        public override string ToString()
        {
            string target;
            if (BoundConnection != null)
            {
                target = $"BoundConnection: {BoundConnection}";
            }
            else if (PartitionId != -1)
            {
                target = $"Partition:{PartitionId}";
            }
            else if (TargetUuid != null)
            {
                target = $"TargetUuid:{TargetUuid}";
            }
            else
            {
                target = "random";
            }
            return
                $"ClientInvocation[clientMessage:{Request}, objectName:{ObjectName}, {target}, sendConnection:{SentConnection}]";
        }

        public Invocation CopyWithNewCorrelationId(long correlationId)
        {
            Request = Request.CopyWithNewCorrelationId(correlationId);
            return this;
        }
    }
}