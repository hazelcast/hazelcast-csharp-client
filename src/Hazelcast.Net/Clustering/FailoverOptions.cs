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

using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents alternative failover cluster options
    /// </summary>
    public class FailoverOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailoverOptions"/>
        /// </summary>
        public FailoverOptions()
        {
            Clusters = new List<ClusterOptions>();
        }

        private FailoverOptions(FailoverOptions options)
        {
            TryCount = options.TryCount;
            Clusters = options.Clusters.Select(p => p.Clone()).ToList();
            Enabled = options.Enabled;
        }

        /// <summary>
        /// Gets or sets Number of tries
        /// <para>It is the number of maximum tries consecutively to connect a cluster. 
        /// Client will fail if number of tries is exhausted. It is reset when connected.</para>
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// Gets or Sets whether <see cref="FailoverOptions"/> enabled. Default is false.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Failover cluster options
        /// </summary>
        public IList<ClusterOptions> Clusters { get; }

        /// <summary>
        /// Clones the options deeply.
        /// </summary>
        /// <returns></returns>
        public FailoverOptions Clone() => new FailoverOptions(this);
    }
}
