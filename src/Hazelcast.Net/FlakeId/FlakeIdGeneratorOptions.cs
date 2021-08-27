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
using System.Threading;

namespace Hazelcast.FlakeId
{
    /// <summary>
    /// Represents options for <see cref="IFlakeIdGenerator"/>.
    /// </summary>
    public class FlakeIdGeneratorOptions
    {
        /// <summary>
        /// Default instance of <see cref="FlakeIdGeneratorOptions"/>.
        /// This is equivalent to <c>new FlakeIdGeneratorOptions()</c> but doesn't allocate additional memory.
        /// </summary>
        public static FlakeIdGeneratorOptions Default { get; } = new FlakeIdGeneratorOptions();

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
        /// Defines how many IDs are pre-fetched on the background when a new flake id is requested from the cluster.
        /// Allowed values are between <c>1</c> and <c>100000</c> inclusive.
        /// Default value is <c>100</c>.
        /// </summary>
        public int PrefetchCount { get; set; } = 100;

        /// <summary>
        /// <para>
        /// Defines for how long the pre-fetched IDs can be used.
        /// If this time elapsed, a new batch of IDs will be fetched.
        /// Default value is <c>10 minutes</c>.
        /// </para>
        /// <para>
        /// The IDs contain timestamp component, which ensures rough global ordering of IDs.
        /// If an ID is assigned to an object that was created much later, it will be much out of order.
        /// If you don't care about ordering, set this value to <see cref="Timeout.InfiniteTimeSpan"/> for unlimited ID validity.
        /// </para>
        /// </summary>
        public TimeSpan PrefetchValidityPeriod { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal FlakeIdGeneratorOptions Clone() => new FlakeIdGeneratorOptions(this);
    }
}
