// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides a base class to CP distributed objects.
    /// </summary>
    internal abstract class CPDistributedObjectBase : ICPDistributedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CPDistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="groupId">The CP group identifier of the object.</param>
        /// <param name="cluster">A cluster.</param>
        protected CPDistributedObjectBase(string serviceName, string name, CPGroupId groupId, Cluster cluster, SerializationService serializationService)
        {
            ServiceName = serviceName;
            Name = name;
            CPGroupId = groupId;
            Cluster = cluster;
            Logger = cluster.State.LoggerFactory.CreateLogger(GetType());
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        protected SerializationService SerializationService { get; }

        protected ILogger Logger { get; }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ICPGroupId GroupId => CPGroupId;

        protected CPGroupId CPGroupId { get; }

        /// <inheritdoc />
        public string PartitionKey => null;

        protected Cluster Cluster { get; }

        protected ValueTask<TObject> ToObjectAsync<TObject>(IData valueData)
        {
            return SerializationService.TryToObject<TObject>(valueData, out var obj, out var state)
                ? new ValueTask<TObject>(obj)
                : SerializationService.ToObjectAsync<TObject>(valueData, state);
        }

        /// <summary>
        /// Sends the invocation to the CP group leader of the cp object if leader connection exists
        /// and <see cref="NetworkingOptions.CPDirectToLeaderEnabled"/> enabled and the cluster is Enterprise;
        /// otherwise sends to any member.
        /// </summary>
        protected Task<ClientMessage> SendCPLeaderAsync(ClientMessage request)
        {
            if (Cluster.State.IsEnterprise && Cluster.State.Options.Networking.CPDirectToLeaderEnabled)
            {
                var leader = Cluster.Members.GetLeaderMemberOf(CPGroupId);

                if (leader != null)
                {
                    return Cluster.Messaging.SendToMemberAsync(request, leader.Uuid);
                }
            }
            else
            {
                if (Cluster.State.Options.Networking.CPDirectToLeaderEnabled)
                    Logger.IfWarning()?.LogWarning("CP Direct To Leader is an Enterprise feature, the request will be sent to any member");
            }

            return Cluster.Messaging.SendAsync(request);
        }

        protected IData ToSafeData(object o1)
        {
            if (o1 == null) throw new ArgumentNullException(nameof(o1));

            var data1 = SerializationService.ToData(o1);
            return data1;
        }

        /// <inheritdoc />
        public virtual async ValueTask DestroyAsync()
        {
            var message = CPGroupDestroyCPObjectCodec.EncodeRequest(CPGroupId, ServiceName, Name);
            var response = await Cluster.Messaging.SendAsync(message).CfAwait();
            _ = CPGroupDestroyCPObjectCodec.DecodeResponse(response);
        }

        /// <summary>
        /// Frees resources used by this instance.
        /// </summary>
        /// <remarks>
        /// Doesn't do anything for now, but some cleanup may be implemented later. <para/>
        /// As such it is recommended to wrap object usage into <code>using</code> statement for better compatibility with future versions.
        /// </remarks>
        public virtual ValueTask DisposeAsync() => default;
    }
}
