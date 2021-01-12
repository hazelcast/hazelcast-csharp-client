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

namespace Hazelcast.Aggregation
{
    /// <summary>
    /// Defines an aggregator that can transform an object into other objects.
    /// </summary>
    /// <typeparam name="TResult">The aggregated result type.</typeparam>
    public interface IAggregator<TResult>
    {
        /// <summary>
        /// Gets the attribute path.
        /// </summary>
        string AttributePath { get; }
    }
}
