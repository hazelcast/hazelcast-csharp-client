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

namespace Hazelcast.Events
{
    /// <summary>
    /// Defines the types of connection lifecycle events.
    /// </summary>
    internal enum ConnectionLifecycleEventType
    {
        /// <summary>
        /// Nothing (default).
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// A connection was added.
        /// </summary>
        Added,

        /// <summary>
        /// A connection was removed.
        /// </summary>
        Removed
    }
}
