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

using System.Collections.Generic;

namespace Hazelcast.Metrics
{
    /// <summary>
    /// Defines a metric descriptor.
    /// </summary>
    internal interface IMetricDescriptor
    {
        /// <summary>
        /// Gets the metric prefix.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Gets the metric attribute prefixes.
        /// </summary>
        /// <remarks>
        /// <para>These prefix are used when writing the descriptor in text form as an attribute i.e. in the
        /// "key=value,key=value,..." human-readable form, in place of the Prefix. If this is <c>null</c>
        /// then Prefix is used.</para>
        /// </remarks>
        string[] AttributePrefixes { get; }

        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the metric discriminator key.
        /// </summary>
        string DiscriminatorKey { get; }

        /// <summary>
        /// Gets the metric discriminator value.
        /// </summary>
        string DiscriminatorValue { get; }

        /// <summary>
        /// Gets the unit of the metric.
        /// </summary>
        MetricUnit Unit { get; }

        /// <summary>
        /// Gets the number of tags.
        /// </summary>
        int TagCount { get; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        IDictionary<string, string> Tags { get; }
    }
}
