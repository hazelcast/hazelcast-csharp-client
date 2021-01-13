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

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents the advanced Near Cache options.
    /// </summary>
    public class CommonNearCacheOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonNearCacheOptions"/> class.
        /// </summary>
        public CommonNearCacheOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonNearCacheOptions"/> class.
        /// </summary>
        private CommonNearCacheOptions(CommonNearCacheOptions other)
        {
            ReconciliationIntervalSeconds = other.ReconciliationIntervalSeconds;
            MinReconciliationIntervalSeconds = other.MinReconciliationIntervalSeconds;
            MaxToleratedMissCount = other.MaxToleratedMissCount;
        }

        /// <summary>
        /// Gets or sets the reconciliation interval.
        /// </summary>
        public int ReconciliationIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the minimum reconciliation interval.
        /// </summary>
        public int MinReconciliationIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum tolerated miss count.
        /// </summary>
        public int MaxToleratedMissCount { get; set; } = 10;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal CommonNearCacheOptions Clone() => new CommonNearCacheOptions(this);
    }
}
