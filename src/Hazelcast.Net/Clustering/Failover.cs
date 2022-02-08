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
using Hazelcast.Configuration;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Failover class holds given alternative failover cluster options. 
    /// It switches the cluster options by listening <see cref="OnClusterDisconnected"/> event handler. On each call of the event, 
    /// tracks the number of trials and switches the current cluster <see cref="CurrentClusterOptions"/> to next one.
    /// </summary>
    internal class Failover
    {
        private readonly ILogger _logger;
        private readonly ClusterState _state;
        private readonly IEnumerable<ClusterOptions> _clusters;
        private readonly IEnumerator<ClusterOptions> _clusterEnumerator;
        private int _currentTryCount = 0;
        private readonly int _maxTryCount;
        private bool _isEnabled;
        private Action<ClusterOptions> _clusterOptionsChanged;

        internal Failover(ClusterState state, IHazelcastOptions options, ILoggerFactory loggerFactory)
        {
            if (options == null || options.Failover == null) throw new ArgumentNullException(nameof(options));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (state == null) throw new ArgumentNullException(nameof(state));

            _logger = loggerFactory.CreateLogger<Failover>();
            _state = state;

            if (!options.Failover.Clusters.Any() && options.Failover.Enabled)
            {
                throw new ConfigurationException("Failover is enabled but there is no cluster(s) provided. Please, chek your configurations.");
            }

            //main cluster config is also an alternative cluster in a circular list (->blue->green->)
            var clusterList = new List<ClusterOptions>();
            clusterList.Add(new ClusterOptions()
            {
                Authentication = options.Authentication,
                ClusterName = options.ClusterName,
                Heartbeat = options.Heartbeat,
                LoadBalancer = options.LoadBalancer,
                Networking = options.Networking,
                WaitForConnectionMilliseconds = options.WaitForConnectionMilliseconds
            });
            clusterList.AddRange(options.Failover.Clusters);
            _clusters = clusterList;

            _clusterEnumerator = _clusters.GetEnumerator();
            ResetToFirstCluster();

            _isEnabled = options.Failover.Enabled;
            _maxTryCount = options.Failover.TryCount;
        }

        /// <summary>
        /// Gets or sets action that will be executed after cluster options are switched
        /// </summary>
        public Action<ClusterOptions> ClusterOptionsChanged
        {
            get => _clusterOptionsChanged;
            set
            {
                _state.ThrowIfPropertiesAreReadOnly();
                _clusterOptionsChanged = value;
            }
        }

        /// <summary>
        /// Handles the event. If <see cref="CanSwitchClusterOptions"/> is true, it switches the <see cref="CurrentClusterOptions"/> to next one.
        /// </summary>
        public void OnClusterDisconnected()
        {
            if (CanSwitchClusterOptions)
            {
                SwitchClusterOptions();
                return;
            }

            _currentTryCount++;
        }

        /// <summary>
        /// Rotates the <see cref="CurrentClusterOptions"/> according to position of the <see cref="_clusterEnumerator"/> 
        /// and triggers the <see cref="ClusterOptionsChanged"/>
        /// </summary>
        private void SwitchClusterOptions()
        {
            _currentTryCount = 0;

            if (!_clusterEnumerator.MoveNext())
            {
                ResetToFirstCluster();
            }

            _clusterOptionsChanged?.Invoke(CurrentClusterOptions);
        }

        private void ResetToFirstCluster()
        {
            _clusterEnumerator.Reset();
            _clusterEnumerator.MoveNext();//request for first item
        }

        /// <summary>
        /// Gets wheather current conditions are suitable to change cluster options to next one.
        /// </summary>
        public bool CanSwitchClusterOptions => _currentTryCount >= _maxTryCount && _isEnabled && _clusters.Any();

        /// <summary>
        /// Gets current <see cref="ClusterOptions" />
        /// </summary>
        public ClusterOptions CurrentClusterOptions => _clusterEnumerator.Current;

        /// <summary>
        /// Gets number of trial for the <see cref="CurrentClusterOptions"/>
        /// </summary>
        public int CurrentTryCount => _currentTryCount;

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
