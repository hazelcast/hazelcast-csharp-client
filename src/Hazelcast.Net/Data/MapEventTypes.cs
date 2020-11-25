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

using System;

namespace Hazelcast.Data
{
    /// <summary>
    /// Specifies the map and entries event types.
    /// </summary>
    [Flags]
    public enum MapEventTypes
    {
        /// <summary>
        /// The entry was added.
        /// </summary>
        Added = 1, // zero is for default, make sure we start at 1

        /// <summary>
        /// The entry was removed.
        /// </summary>
        Removed = 1 << 1,

        /// <summary>
        /// The entry was updated.
        /// </summary>
        Updated = 1 << 2,

        /// <summary>
        /// The entry was evicted.
        /// </summary>
        Evicted = 1 << 3,

        /// <summary>
        /// The entry has expired.
        /// </summary>
        Expired = 1 << 4,

        /// <summary>
        /// All entries were evicted.
        /// </summary>
        AllEvicted = 1 << 5,

        /// <summary>
        /// All entries were cleared.
        /// </summary>
        AllCleared = 1 << 6,

        /// <summary>
        /// The entry was merged.
        /// </summary>
        Merged = 1 << 7,

        /// <summary>
        /// The entry was invalidated.
        /// </summary>
        Invalidated = 1 << 8,

        /// <summary>
        /// The entry was loaded.
        /// </summary>
        Loaded = 1 << 9
    }
}
