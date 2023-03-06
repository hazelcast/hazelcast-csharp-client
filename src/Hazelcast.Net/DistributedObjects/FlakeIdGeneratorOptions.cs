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
using System.Threading;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents options for <see cref="IFlakeIdGenerator"/>.
    /// </summary>
    public class FlakeIdGeneratorOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlakeIdGeneratorOptions"/> class.
        /// </summary>
        public FlakeIdGeneratorOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlakeIdGeneratorOptions"/> class.
        /// </summary>
        /// <param name="other">Instance to copy option values from.</param>
        private FlakeIdGeneratorOptions(FlakeIdGeneratorOptions other)
        {
            PrefetchCount = other.PrefetchCount;
            PrefetchValidityPeriod = other.PrefetchValidityPeriod;
        }

        /// <summary>
        /// Gets or sets the number of identifiers in a pre-fetch batch.
        /// </summary>
        /// <remarks>
        /// <para>Allowed values are between <c>1</c> and <c>100000</c> inclusive, default value is <c>100</c>.</para>
        /// <para>When an identifier is initially requested, an entire batch of identifiers is fetched from the cluster,
        /// and this option configures the number of identifiers in such a batch.</para>
        /// </remarks>
        public int PrefetchCount { get; set; } = 100;

        /// <summary>
        /// Gets or sets the validity period of identifier batches.
        /// </summary>
        /// <remarks>
        /// <para>When an identifier is initially requested, an entire batch of identifier is fetched from the cluster.
        /// This option configures how long these identifiers can be used, before the batch is dropped and a new
        /// batch is fetched.</para>
        /// <para>Fetched identifiers contain a timestamp component which ensures rough global ordering of identifiers.
        /// If an identifier is used a long time after it was fetched, the chances it is highly out-of-order increase. Set
        /// this option according your usage pattern. If you do not care about ordering, set this option to
        /// <see cref="Timeout.InfiniteTimeSpan" /> to achieve infinite validity.</para>
        /// </remarks>
        public TimeSpan PrefetchValidityPeriod { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal FlakeIdGeneratorOptions Clone() => new FlakeIdGeneratorOptions(this);
    }
}
