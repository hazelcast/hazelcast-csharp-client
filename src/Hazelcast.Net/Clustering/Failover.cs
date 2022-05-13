// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Failover class holds given alternative failover cluster options. 
    /// It switches the cluster options by listening <see cref="OnClusterStateChanged"/> event handler. On each call of the event, 
    /// tracks the number of trials and switches the current cluster <see cref="CurrentClusterOptions"/> to next one.
    /// </summary>
    internal class Failover
    {
        private readonly ClusterState _state;
        private readonly IEnumerable<HazelcastOptions> _clusters;
        private readonly IEnumerator<HazelcastOptions> _clusterEnumerator;
        private int _currentTryCount;
        private readonly int _maxTryCount;
        private bool _isEnabled;
        private Action<HazelcastOptions> _clusterOptionsChanged;

        internal Failover(ClusterState state, HazelcastOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (state == null) throw new ArgumentNullException(nameof(state));

            _state = state;
            _isEnabled = options.FailoverOptions.Enabled;

            if (Enabled)
            {
                _clusters = new List<HazelcastOptions>(options.FailoverOptions.Clusters);
            }
            else
            {   //there is one cluster config, failover disabled.
                _clusters = new List<HazelcastOptions>() { options };
            }

            _maxTryCount = options.FailoverOptions.TryCount;

            _clusterEnumerator = _clusters.GetEnumerator();
            ResetToFirstCluster();

            HConsole.Configure(x => x.Configure<Failover>().SetPrefix("CLUST.FAILOVER"));
        }

        /// <summary>
        /// Triggers when failover is possible, and cluster options are changed.
        /// </summary>
        public Action<HazelcastOptions> ClusterOptionsChanged
        {
            get => _clusterOptionsChanged;
            set
            {
                _clusterOptionsChanged = value;
            }
        }

        /// <summary>
        /// Handles the event. If <see cref="CanSwitchClusterOptions"/> is true, and state is disconnected, it switches the <see cref="CurrentClusterOptions"/> to next one.
        /// </summary>
        public ValueTask OnClusterStateChanged(ClientState clientState)
        {
            //We have connected, reset the counter.
            if (clientState == ClientState.Connected)
            {
                _currentTryCount = 0;
            }

            return default;
        }

        /// <summary>
        /// Changes the cluster options, and triggers <see cref="ClusterOptionsChanged"/>
        /// </summary>
        /// <remarks>Failover can only work when client state is <see cref="ClientState.Disconnected"/></remarks>
        /// <returns>true if options changed, otherwise false</returns>
        public bool RequestClusterChange()
        {
            if ((_state.ClientState == ClientState.Disconnected || _state.ClientState == ClientState.Started) &&
                CanSwitchClusterOptions)
            {
                SwitchClusterOptions();
                HConsole.WriteLine(this, "CLUSTER OPTIONS SWITCHED");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rotates the <see cref="CurrentClusterOptions"/> according to position of the <see cref="_clusterEnumerator"/> 
        /// and triggers the <see cref="ClusterOptionsChanged"/>
        /// </summary>
        private void SwitchClusterOptions()
        {
            if (!_clusterEnumerator.MoveNext())
            {
                ResetToFirstCluster();
                _currentTryCount++;
            }

            _clusterOptionsChanged?.Invoke(CurrentClusterOptions);
        }

        private void ResetToFirstCluster()
        {
            _clusterEnumerator.Reset();
            _clusterEnumerator.MoveNext();//request for first item            
        }

        /// <summary>
        /// Gets whether current conditions are suitable to change cluster options to next one.
        /// </summary>
        public bool CanSwitchClusterOptions => _currentTryCount < _maxTryCount && _isEnabled && _clusters.Any();

        /// <summary>
        /// Gets current <see cref="ClusterOptions" />
        /// </summary>
        public HazelcastOptions CurrentClusterOptions => _clusterEnumerator.Current;

        /// <summary>
        /// Gets number of trial for the <see cref="CurrentClusterOptions"/>
        /// </summary>
        public int CurrentTryCount => _currentTryCount;

        /// <summary>
        /// Gets whether current cluster options set by Failover, 
        /// and client will establish a connection to a backup cluster.
        /// </summary>
        public bool IsChangingCluster => _currentTryCount > 0;

        /// <summary>
        /// Gets or sets <see cref="Failover"/> whether is enabled.
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
