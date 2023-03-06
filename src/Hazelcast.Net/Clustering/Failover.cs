// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Manages the clusters failover.
    /// </summary>
    internal class Failover
    {
        private readonly ClusterState _state;
        private readonly List<HazelcastOptions> _clusters;
        private readonly IEnumerator<HazelcastOptions> _clusterEnumerator;
        private readonly int _maxCount;
        private int _count;
        private bool _isEnabled;
        private Action<HazelcastOptions> _clusterChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Failover"/> class.
        /// </summary>
        internal Failover(ClusterState state, HazelcastOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _state = state ?? throw new ArgumentNullException(nameof(state));
            _isEnabled = options.FailoverOptions.Enabled; // TODO why 'enabled' in options ?!

            // no failover if only one cluster is declared in the failover configuration
            _clusters = Enabled
                ? new List<HazelcastOptions>(options.FailoverOptions.Clients)
                : new List<HazelcastOptions> { options };

            // total count is try-count (the whole list) times the number of clusters
            // so for instance if we have 3 clusters and try-count is 2, we can switch
            // to another cluster 6 times and then we have to give up
            _maxCount = options.FailoverOptions.TryCount * _clusters.Count;

            // enumerate clusters, go to the first cluster
            _clusterEnumerator = _clusters.GetEnumerator(); // TODO: dispose the enumerator
            _clusterEnumerator.MoveNext();

            HConsole.Configure(x => x.Configure<Failover>().SetPrefix("CLUST.FAILOVER"));
        }

        /// <summary>
        /// Triggers when a new set of cluster options is selected.
        /// </summary>
        public Action<HazelcastOptions> ClusterChanged
        {
            get => _clusterChanged;
            set
            {
                // TODO we should lock on readonly properties but at the moment it fails
                _clusterChanged = value;
            }
        }

        /// <summary>
        /// Handles the event and resets the try count upon connection.
        /// </summary>
        public ValueTask OnClusterStateChanged(ClientState clientState)
        {
            // we have connected, reset the counter
            if (clientState == ClientState.Connected)
                _count = 0;

            return default;
        }

        /// <summary>
        /// Tries to switch to the next cluster.
        /// </summary>
        /// <remarks>
        /// <para>Failover can only switch to the next cluster when the client state is either
        /// <see cref="ClientState.Disconnected"/> or <see cref="ClientState.Started"/>.</para>
        /// </remarks>
        /// <returns><c>true</c> if successfully switched to the next cluster; otherwise <c>false</c>.</returns>
        public bool TryNextCluster()
        {
            // only disconnected if client state is Disconnected or Started
            if (_state.ClientState != ClientState.Disconnected && _state.ClientState != ClientState.Started)
                return false;

            // TODO: this should be validated in the builder, not here?
            if (!_isEnabled || _clusters.Count == 0)
                return false;

            if (_count == _maxCount) return false;

            _count += 1;

            if (!_clusterEnumerator.MoveNext()) // rotate the list
            {
                _clusterEnumerator.Reset();
                _clusterEnumerator.MoveNext();
            }

            _clusterChanged?.Invoke(CurrentClusterOptions);

            return true;
        }

        /// <summary>
        /// Gets current cluster options.
        /// </summary>
        public HazelcastOptions CurrentClusterOptions => _clusterEnumerator.Current;

        /// <summary>
        /// Gets number of times we have switched to a new cluster.
        /// </summary>
        public int CurrentTryCount => _count;

        /// <summary>
        /// Determines whether the client is currently falling over to another cluster.
        /// </summary>
        public bool IsChangingCluster => _count > 0;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Failover"/> is enabled.
        /// </summary>
        public bool Enabled
        {
            get => _isEnabled;
            set
            {
                _state.ThrowIfPropertiesAreReadOnly();
                _isEnabled = value;
            }
        }
    }
}
