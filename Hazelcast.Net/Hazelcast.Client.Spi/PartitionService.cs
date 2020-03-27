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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;
using static Hazelcast.Util.ValidationUtil;

namespace Hazelcast.Client.Spi
{
    public class PartitionService : IPartitionService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(PartitionService));
        private readonly HazelcastClient _client;

        private readonly AtomicReference<PartitionTable> _partitionTable =
            new AtomicReference<PartitionTable>(new PartitionTable());

        private readonly AtomicInteger _partitionCount = new AtomicInteger(0);
        private readonly bool _isSmart;


        internal PartitionTable InternalPartitionTable { get; }
        
        internal PartitionService(HazelcastClient client)
        {
            _client = client;
            _isSmart = client.Configuration.NetworkConfig.SmartRouting;
        }

        internal void Start()
        {
        }

        internal void HandlePartitionsViewEvent(Connection connection, ICollection<KeyValuePair<Guid, IList<int>>> partitions,
            int partitionStateVersion)
        {
            if (Logger.IsFinestEnabled)
            {
                Logger.Finest($"Handling new partition table with  partitionStateVersion: {partitionStateVersion}");
            }
            while (true)
            {
                var current = _partitionTable.Get();
                if (!ShouldBeApplied(connection, partitions, partitionStateVersion, current))
                {
                    return;
                }
                var newPartitions = ConvertToMap(partitions);
                var newMetaData = new PartitionTable
                {
                    Connection = connection, PartitionSateVersion = partitionStateVersion, Partitions = newPartitions
                };
                if (_partitionTable.CompareAndSet(current, newMetaData))
                {
                    if (Logger.IsFinestEnabled)
                    {
                        Logger.Finest("Applied partition table with partitionStateVersion : " + partitionStateVersion);
                    }
                    return;
                }
            }
        }

        private ConcurrentDictionary<int, Guid> ConvertToMap(ICollection<KeyValuePair<Guid, IList<int>>> partitions)
        {
            var newPartitions = new ConcurrentDictionary<int, Guid>();
            foreach (var entry in partitions)
            {
                var guid = entry.Key;
                foreach (var partition in entry.Value)
                {
                    newPartitions.TryAdd(partition, guid);
                }
            }
            return newPartitions;
        }


        private bool ShouldBeApplied(Connection connection, ICollection<KeyValuePair<Guid, IList<int>>> partitions,
            int partitionStateVersion, PartitionTable current)
        {
            if (partitions.Count == 0)
            {
                if (Logger.IsFinestEnabled)
                {
                    LogFailure(connection, partitionStateVersion, current, "response is empty");
                }
                return false;
            }
            if (!connection.Equals(current?.Connection))
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest(
                        $"Event coming from a new connection. Old connection: {current?.Connection} , new connection {connection}");
                }
                return true;
            }
            if (partitionStateVersion <= current?.PartitionSateVersion)
            {
                if (Logger.IsFinestEnabled)
                {
                    LogFailure(connection, partitionStateVersion, current, "response state version is old");
                }
                return false;
            }
            return true;
        }

        private static void LogFailure(Connection connection, int partitionStateVersion, PartitionTable current, string cause)
        {
            Logger.Finest($" We will not apply the response, because {cause} . " + $"Response is from {connection}. " +
                          $"Current connection {current?.Connection} response state version:{partitionStateVersion}. " +
                          $"Current state version: {current?.PartitionSateVersion}");
        }

        internal Guid? GetPartitionOwner(int partitionId)
        {
            var partitions = _partitionTable.Get()?.Partitions;
            if (partitions != null && partitions.TryGetValue(partitionId, out var returnValue))
            {
                return returnValue;
            }
            return null;
        }

        internal int GetPartitionId(IData key)
        {
            var pc = GetPartitionCount();
            var hash = key.GetPartitionHash();
            return HashUtil.HashToIndex(hash, pc);
        }

        internal int GetPartitionId(object key)
        {
            var data = _client.SerializationService.ToData(key);
            return GetPartitionId(data);
        }

        internal int GetPartitionCount()
        {
            return _partitionCount.Get();
        }

        internal ISet<IPartition> GetPartitions()
        {
            var partitionCount = GetPartitionCount();
            var partitions = new HashSet<IPartition>();
            for (var i = 0; i < partitionCount; i++)
            {
                var partition = GetPartition(i);
                partitions.Add(partition);
            }
            return partitions;
        }

        internal IPartition GetPartition(object key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var partitionId = GetPartitionId(key);
            return GetPartition(partitionId);
        }

        internal IPartition GetPartition(int partitionId)
        {
            return new Partition(_client, partitionId);
        }

        internal Guid AddPartitionLostListener(IPartitionLostListener partitionLostListener)
        {
            CheckNotNull(partitionLostListener, NullListenerIsNotAllowed);
            var request = ClientAddPartitionLostListenerCodec.EncodeRequest(_isSmart);

            void HandlePartitionLostEvent(int partitionId, int lostBackupCount, Guid source)
            {
                var member = _client.ClusterService.GetMember(source);
                partitionLostListener.PartitionLost(new PartitionLostEvent(partitionId, lostBackupCount, member.Address));
            }

            void EventHandler(ClientMessage eventMessage) =>
                ClientAddPartitionLostListenerCodec.EventHandler.HandleEvent(eventMessage, HandlePartitionLostEvent);

            Guid ResponseDecoder(ClientMessage response) => ClientAddPartitionLostListenerCodec.DecodeResponse(response).Response;

            ClientMessage EncodeDeregisterRequest(Guid registrationId) =>
                ClientRemovePartitionLostListenerCodec.EncodeRequest(registrationId);

            return _client.ListenerService.RegisterListener(request, ResponseDecoder, EncodeDeregisterRequest, EventHandler);
        }

        internal bool RemovePartitionLostListener(Guid registrationId)
        {
            return _client.ListenerService.DeregisterListener(registrationId);
        }

        IPartition IPartitionService.GetPartition(object key)
        {
            return GetPartition(key);
        }

        Guid IPartitionService.AddPartitionLostListener(IPartitionLostListener partitionLostListener)
        {
            return AddPartitionLostListener(partitionLostListener);
        }

        bool IPartitionService.RemovePartitionLostListener(Guid registrationId)
        {
            return RemovePartitionLostListener(registrationId);
        }

        ISet<IPartition> IPartitionService.GetPartitions()
        {
            return GetPartitions();
        }

        internal class PartitionTable
        {
            public PartitionTable()
            {
                Connection = null;
                PartitionSateVersion = -1;
                Partitions = new ConcurrentDictionary<int, Guid>();
            }

            public Connection Connection { get; set; }
            public int PartitionSateVersion { get; set; }
            public ConcurrentDictionary<int, Guid> Partitions { get; set; }
        }

        internal class Partition : IPartition
        {
            private readonly HazelcastClient _client;

            public Partition(HazelcastClient client, int partitionId)
            {
                _client = client;
                PartitionId = partitionId;
            }

            public int PartitionId { get; }

            public IMember PartitionOwner
            {
                get
                {
                    var owner = _client.PartitionService.GetPartitionOwner(PartitionId);
                    if (owner == null)
                    {
                        var message = ClientTriggerPartitionAssignmentCodec.EncodeRequest();
                        _client.InvocationService.InvokeOnRandomTarget(message);
                        return null;
                    }
                    return _client.ClusterService.GetMember(owner.Value);
                }
            }

            public override string ToString()
            {
                return $"Partition[partitionId={PartitionId}]";
            }
        }

        internal class PartitionLostEvent : IPartitionLostEvent
        {
            private const int MaxBackCount = 6;

            public PartitionLostEvent(int partitionId, int lostBackupCount, Address eventSource)
            {
                PartitionId = partitionId;
                LostBackupCount = lostBackupCount;
                EventSource = eventSource;
            }

            public int PartitionId { get; }
            public int LostBackupCount { get; }

            public bool IsAllReplicasInPartitionLost => LostBackupCount == MaxBackCount;

            public Address EventSource { get; }
        }

        public bool CheckAndSetPartitionCount(int newPartitionCount)
        {
            if (_partitionCount.CompareAndSet(0, newPartitionCount))
            {
                return true;
            }
            return _partitionCount.Get() == newPartitionCount;
        }
    }
}