// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Provides a base class for all event arguments.
    /// </summary>
    public abstract class EventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgsBase"/> class.
        /// </summary>
        /// <param name="state">A state object.</param>
        protected EventArgsBase(object state)
        {
            State = state;
        }

        /// <summary>
        /// Gets the state object.
        /// </summary>
        public object State { get; }
    }
}
