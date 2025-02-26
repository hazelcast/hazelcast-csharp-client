// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast
{
    /// <summary>
    /// Represents a starting <see cref="IHazelcastClient"/>.
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types - we're not comparing them
    public readonly struct HazelcastClientStart
#pragma warning restore CA1815
    {
        internal HazelcastClientStart(IHazelcastClient client, Task task)
        {
            Client = client;
            Task = task;
        }

        /// <summary>
        /// Gets the <see cref="IHazelcastClient"/>.
        /// </summary>
        public IHazelcastClient Client { get; }

        /// <summary>
        /// Gets the task which will complete when the client has started, or fail if the client fails to start.
        /// </summary>
        public Task Task { get; }
    }
}