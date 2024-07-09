// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    internal class ClusterCPGroups : IAsyncDisposable
    {
        private long _version = -1;
        private readonly object _mutex = new();
        private readonly object _listenerMutex = new();
        private readonly ConcurrentDictionary<CPGroupId, CPGroupInfo> _groups = new();
        private readonly ILogger _logger;
        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private int _disposed;
        private MemberConnection _listenerConnection;
        private Task _listenerRegisterTask;

        private CancellationTokenSource _cts;

        public ClusterCPGroups(ClusterState clusterState, ClusterMembers members)
        {
            _clusterState = clusterState;
            _clusterMembers = members;
            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterCPGroups>();
        }

        public long Version => _version;

        public void SetGroups(long version, ICollection<CPGroupInfo> groups)
        {
            if (_version >= version)
                return;

            var oldCount = _groups.Count;
            lock (_mutex)
            {
                _version = version;
                _groups.Clear();
                foreach (var group in groups)
                {
                    _groups[group.GroupId] = group;
                }
            }

            _logger.IfDebug()?.LogDebug("CP groups updated to Version {Version}." +
                                        " Old groups count: {OldCount}, new count: {NewCount}", Version, oldCount, groups.Count);
        }

        public CPGroupInfo GetGroup(CPGroupId groupId) => _groups.TryGetValue(groupId, out var group) ? group : null;

        public bool TryGetGroup(CPGroupId groupId, out CPGroupInfo group) => _groups.TryGetValue(groupId, out group);


        #region EventHandlers

        // Since CP part is wanted to be keep seperated from regular client, the view listener is handled here.
        // In the future, if we need other type of listeners, we can refactor ClusterEvents to support multiple listeners.

        public ValueTask OnConnectionOpened(MemberConnection connection, bool isFirstEver, bool isFirst, bool isNewCluster, ClusterVersion clusterVersion)
        {
            lock (_listenerMutex)
            {
                if (_listenerConnection == null)
                    _listenerRegisterTask ??= RegisterViewListenerAsync(connection, _cts.Token);
            }
            return default;
        }
        private async Task RegisterViewListenerAsync(MemberConnection connection, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    MemberConnection validConnection = connection ?? _clusterMembers.GetRandomConnection();

                    if (await TryRegisterViewListenerAsync(validConnection).CfAwait())
                        break;
                }
            }
            finally
            {
                // If the task is cancelled, it means the object is disposed.
                // If the task is completed, it means the listener is registered.
                // In both cases, we don't need to keep the task reference.
                _listenerRegisterTask = null;
            }
        }
        private async Task<bool> TryRegisterViewListenerAsync(MemberConnection connection)
        {
            try
            {
                var request = ClientAddCPGroupViewListenerCodec.EncodeRequest();
                _ = await connection.SendAsync(request).CfAwait();
                _logger.IfDebug()?.LogDebug("CP group view listener added to {Connection}", connection.Id);

                return true;
            }
            catch (Exception e) when (e is TargetDisconnectedException or ClientOfflineException)
            {
                _logger.IfDebug()?.LogDebug("Failed to subscribe to cp group view on connection {ConnectionId)} ({Reason}), may retry.", connection.Id.ToShortString(),
                    e is ClientOfflineException o ? ("offline, " + o.State) : "disconnected");
            }
            catch (Exception ex)
            {
                _logger.IfDebug()?.LogDebug("Failed to subscribe to cp group view on connection {ConnectionId)} ({Reason}), may retry.", connection.Id.ToShortString(), ex);
            }

            return false;
        }

        public ValueTask OnConnectionClosed(MemberConnection connection)
        {
            lock (_listenerMutex)
            {
                if (connection != _listenerConnection) return default;

                _listenerConnection = null;
                _listenerRegisterTask ??= RegisterViewListenerAsync(null, _cts.Token);
            }

            return default;
        }

        #endregion
        
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;
            _cts.Cancel();
            await _listenerRegisterTask.MaybeNull().CfAwaitCanceled();
        }
    }
}
