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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;
namespace Hazelcast.CP
{
    /// <summary>
    /// Manages CP group information in the cluster.
    /// </summary>
    internal class ClusterCPGroups
    {
        private long _version = InitialVersion;
        private readonly object _mutex = new();
        private readonly object _listenerMutex = new();
        private readonly ConcurrentDictionary<CPGroupId, Guid> _groups = new();
        private readonly ILogger _logger;
        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private int _disposed;
        private MemberConnection _listenerConnection;
        private Task _listenerRegisterTask;
        private long _correlationId;

        private CancellationTokenSource _cts;

        public const string CPGroupsJsonField = "cp.leaders";
        public const long InitialVersion = -1;

        public ClusterCPGroups(ClusterState clusterState, ClusterMembers members)
        {
            _clusterState = clusterState;
            _clusterMembers = members;
            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterCPGroups>();
        }

        public long Version => _version;
        public int Count => _groups.Count;

        public void SetGroups(long version, ICollection<CPGroupInfo> groups, IList<KeyValuePair<Guid, Guid>> cpToApUuids)
        {
            if (_version >= version)
                return;

            var oldCount = _groups.Count;

            var newGroup = MapCPtoAPUuids(cpToApUuids, groups);

            lock (_mutex)
            {
                // One more check in safe env.
                if (_version >= version)
                    return;

                _version = version;

                _groups.Clear();
                foreach (var g in newGroup)
                {
                    _groups[g.Key] = g.Value;
                }

            }

            _logger.IfDebug()?.LogDebug("CP groups updated to Version {Version}." +
                                        " Old groups count: {OldCount}, new count: {NewCount}", Version, oldCount, groups.Count);
        }
        private Dictionary<CPGroupId, Guid> MapCPtoAPUuids(IList<KeyValuePair<Guid, Guid>> cpToApUuids, ICollection<CPGroupInfo> groupInfos)
        {
            var mapIds = new Dictionary<CPGroupId, Guid>();

            var leaderIds = cpToApUuids.ToDictionary(x => x.Key, x => x.Value);

            foreach (var group in groupInfos)
            {
                if (leaderIds.TryGetValue(group.Leader.Uuid, out var apUuid))
                {
                    mapIds[group.GroupId] = apUuid;
                }
            }

            return mapIds;
        }


        public Guid GetLeaderMemberId(CPGroupId groupId) => _groups.TryGetValue(groupId, out var memberId) ? memberId : Guid.Empty;

        public bool TryGetLeaderMemberId(CPGroupId groupId, out Guid group) => _groups.TryGetValue(groupId, out group);

        /// <summary>
        /// Overrides current leader mappings with the given ones.
        /// Suitable for first time leader mappings.
        /// </summary>
        /// <param name="cpGroupLeaders">Mapped CP Group Ids and their leaders</param>
        public void SetCPGroupIds(IDictionary<CPGroupId, Guid> cpGroupLeaders)
        {
            lock (_mutex)
            {
                if(cpGroupLeaders.Count == 0) return;
                
                _version = InitialVersion;
                _groups.Clear();
                foreach (var entry in cpGroupLeaders)
                {
                    _groups[entry.Key] = entry.Value;
                }
            }
        }
    }
}

